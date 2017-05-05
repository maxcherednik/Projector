using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Join
{
    public class ResultSelectorVisitor : ExpressionVisitor
    {
        private string _currentProjectedName;
        private ParameterExpression _schemaParameterLeft;
        private ParameterExpression _idParameterLeft;
        private ParameterExpression _schemaParameterRight;
        private ParameterExpression _idParameterRight;
        private MethodInfo _getFieldMethodInfo;

        private Dictionary<ParameterExpression, Tuple<ParameterExpression, ParameterExpression, Dictionary<string, ISet<string>>>> _parametersToScheamMap;

        public ResultSelectorVisitor()
        {
            _schemaParameterLeft = Expression.Parameter(typeof(ISchema), "schema");
            _idParameterLeft = Expression.Parameter(typeof(int), "id");

            _schemaParameterRight = Expression.Parameter(typeof(ISchema), "schema");
            _idParameterRight = Expression.Parameter(typeof(int), "id");

            _parametersToScheamMap = new Dictionary<ParameterExpression, Tuple<ParameterExpression, ParameterExpression, Dictionary<string, ISet<string>>>>(2);

            _getFieldMethodInfo = typeof(ISchema).GetTypeInfo().GetDeclaredMethod("GetField");

        }

        public JoinProjectedFieldsMeta GenerateProjection<TLeft, TRight, TResult>(Expression<Func<TLeft, TRight, TResult>> transformerExpression)
        {
            var oldFieldNamesToNewFieldNamesMappingLeft = new Dictionary<string, ISet<string>>();
            var oldFieldNamesToNewFieldNamesMappingRight = new Dictionary<string, ISet<string>>();

            _parametersToScheamMap.Add(transformerExpression.Parameters[0], Tuple.Create(_schemaParameterLeft, _idParameterLeft, oldFieldNamesToNewFieldNamesMappingLeft));
            _parametersToScheamMap.Add(transformerExpression.Parameters[1], Tuple.Create(_schemaParameterRight, _idParameterRight, oldFieldNamesToNewFieldNamesMappingRight));

            var projectedFields = GenerateFromAnonymous((NewExpression)transformerExpression.Body);

            return new JoinProjectedFieldsMeta(oldFieldNamesToNewFieldNamesMappingLeft, oldFieldNamesToNewFieldNamesMappingRight, projectedFields);
        }

        private IField GenerateField(string projectedFieldName, Type typeOfValue, Expression expression)
        {
            _currentProjectedName = projectedFieldName;
            var projectedFieldType = typeof(JoinProjectedField<>).MakeGenericType(typeOfValue);

            var typeOfFunc = typeof(Func<,,,,>).MakeGenericType(typeof(ISchema), typeof(int), typeof(ISchema), typeof(int), typeOfValue);

            var lambda = Expression.Lambda(typeOfFunc, Visit(expression), _schemaParameterLeft, _idParameterLeft, _schemaParameterRight, _idParameterRight);

            var projectedField = Activator.CreateInstance(projectedFieldType, projectedFieldName, lambda.Compile());

            return (IField)projectedField;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var projectedFieldName = node.Member.Name;
            var typeOfvalue = node.Expression.Type;

            GenerateField(projectedFieldName, typeOfvalue, node.Expression);

            return node;
        }

        protected IDictionary<string, IField> GenerateFromAnonymous(NewExpression node)
        {
            // this is for anonymous types only
            var fieldDict = new Dictionary<string, IField>(node.Members.Count);

            for (int i = 0; i < node.Members.Count; i++)
            {
                var member = node.Members[i];
                var valueExpression = node.Arguments[i];
                var projectedFieldName = member.Name;
                var typeOfvalue = valueExpression.Type;

                fieldDict.Add(projectedFieldName, GenerateField(projectedFieldName, typeOfvalue, valueExpression));
            }

            return fieldDict;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var schemaParameter = _parametersToScheamMap[(ParameterExpression)node.Expression].Item1;
            var idParameter = _parametersToScheamMap[(ParameterExpression)node.Expression].Item2;
            var oldFieldNamesToNewFieldNamesMapping = _parametersToScheamMap[(ParameterExpression)node.Expression].Item3;

            var oldFieldName = node.Member.Name;
            if (!oldFieldNamesToNewFieldNamesMapping.TryGetValue(oldFieldName, out ISet<string> newFieldNames))
            {
                newFieldNames = new HashSet<string>();
                oldFieldNamesToNewFieldNamesMapping.Add(oldFieldName, newFieldNames);
            }

            newFieldNames.Add(_currentProjectedName);

            var genericMethodInfo = _getFieldMethodInfo.MakeGenericMethod(node.Type);
            var fieldAccessExpression = Expression.Call(schemaParameter, genericMethodInfo, Expression.Constant(oldFieldName, typeof(string)));

            var valueAccessMemberInfo = genericMethodInfo.ReturnType.GetTypeInfo().GetDeclaredMethod("GetValue");

            var valueAccessExpression = Expression.Call(fieldAccessExpression, valueAccessMemberInfo, idParameter);
            return valueAccessExpression;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Join
{
    public class ResultSelectorVisitor : ExpressionVisitor
    {
        private string _currentProjectedName;

        private IDictionary<string, IField> _projectedFields;
        private IDictionary<string, ISet<string>> _oldFieldNamesToNewFieldNamesMapping;
        private ParameterExpression _schemaParameterLeft;
        private ParameterExpression _idParameterLeft;
        private ParameterExpression _schemaParameterRight;
        private ParameterExpression _idParameterRight;
        private MethodInfo _getFieldMethodInfo;
        private bool _skip;

        private Dictionary<ParameterExpression, Tuple<ParameterExpression, ParameterExpression>> _parametersToScheamMap;

        public ResultSelectorVisitor()
        {
            _schemaParameterLeft = Expression.Parameter(typeof(ISchema), "schema");
            _idParameterLeft = Expression.Parameter(typeof(int), "id");

            _schemaParameterRight = Expression.Parameter(typeof(ISchema), "schema");
            _idParameterRight = Expression.Parameter(typeof(int), "id");

            _parametersToScheamMap = new Dictionary<ParameterExpression, Tuple<ParameterExpression, ParameterExpression>>(2);

            _getFieldMethodInfo = typeof(ISchema).GetTypeInfo().GetDeclaredMethod("GetField");
            _projectedFields = new Dictionary<string, IField>();
            _oldFieldNamesToNewFieldNamesMapping = new Dictionary<string, ISet<string>>();
        }

        public Tuple<IDictionary<string, ISet<string>>, IDictionary<string, IField>> GenerateProjection<TLeft, TRight, TResult>(Expression<Func<TLeft, TRight, TResult>> transformerExpression)
        {
            _projectedFields.Clear();

            Visit(transformerExpression);

            return Tuple.Create(_oldFieldNamesToNewFieldNamesMapping, _projectedFields);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            _parametersToScheamMap.Add(node.Parameters[0], Tuple.Create(_schemaParameterLeft, _idParameterLeft));
            _parametersToScheamMap.Add(node.Parameters[1], Tuple.Create(_schemaParameterRight, _idParameterRight));

            return base.VisitLambda(node);
        }

        private void GenerateField(string projectedFieldName, Type typeOfValue, Expression expression)
        {
            _currentProjectedName = projectedFieldName;
            var projectedFieldType = typeof(JoinProjectedField<>).MakeGenericType(typeOfValue);

            var typeOfFunc = typeof(Func<,,,,>).MakeGenericType(typeof(ISchema), typeof(int), typeof(ISchema), typeof(int), typeOfValue);

            var lambda = Expression.Lambda(typeOfFunc, Visit(expression), _schemaParameterLeft, _idParameterLeft, _schemaParameterRight, _idParameterRight);

            var projectedField = Activator.CreateInstance(projectedFieldType, projectedFieldName, lambda.Compile());

            _projectedFields.Add(projectedFieldName, (IField)projectedField);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var projectedFieldName = node.Member.Name;
            var typeOfvalue = node.Expression.Type;

            GenerateField(projectedFieldName, typeOfvalue, node.Expression);

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            // this is for anonymous types only
            if (node.Members != null)
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    var member = node.Members[i];
                    var valueExpression = node.Arguments[i];
                    var projectedFieldName = member.Name;
                    var typeOfvalue = valueExpression.Type;

                    GenerateField(projectedFieldName, typeOfvalue, valueExpression);
                }

                _skip = true;
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (_skip)
            {
                return base.VisitMember(node);
            }

            var schemaParameter = _parametersToScheamMap[(ParameterExpression)node.Expression].Item1;
            var idParameter = _parametersToScheamMap[(ParameterExpression)node.Expression].Item2;


            var oldFieldName = node.Member.Name;
            if (!_oldFieldNamesToNewFieldNamesMapping.TryGetValue(oldFieldName, out ISet<string> newFieldNames))
            {
                newFieldNames = new HashSet<string>();
                _oldFieldNamesToNewFieldNamesMapping.Add(oldFieldName, newFieldNames);
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

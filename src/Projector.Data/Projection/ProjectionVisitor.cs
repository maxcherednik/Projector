using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Projection
{
    public class ProjectionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _schemaParameter;
        private readonly ParameterExpression _idParameter;
        private readonly MethodInfo _getFieldMethodInfo;
        private readonly IDictionary<string, IField> _projectedFields;
        private readonly IDictionary<string, ISet<string>> _oldFieldNamesToNewFieldNamesMapping;
        private string _currentProjectedName;
        private bool _skip;

        public ProjectionVisitor()
        {
            _schemaParameter = Expression.Parameter(typeof(ISchema), "schema");
            _idParameter = Expression.Parameter(typeof(int), "id");

            _getFieldMethodInfo = typeof(ISchema).GetTypeInfo().GetDeclaredMethod("GetField");
            _projectedFields = new Dictionary<string, IField>();
            _oldFieldNamesToNewFieldNamesMapping = new Dictionary<string, ISet<string>>();
        }

        public Tuple<IDictionary<string, ISet<string>>, IDictionary<string, IField>> GenerateProjection<TSource, TDest>(Expression<Func<TSource, TDest>> transformerExpression)
        {
            _projectedFields.Clear();

            Visit(transformerExpression);

            return Tuple.Create(_oldFieldNamesToNewFieldNamesMapping, _projectedFields);
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var projectedFieldName = node.Member.Name;
            var typeOfvalue = node.Expression.Type;

            GenerateField(projectedFieldName, typeOfvalue, node.Expression);

            return node;
        }

        private void GenerateField(string projectedFieldName, Type typeOfValue, Expression expression)
        {
            _currentProjectedName = projectedFieldName;
            var projectedFieldType = typeof(ProjectedField<>).MakeGenericType(typeOfValue);

            var typeOfFunc = typeof(Func<,,>).MakeGenericType(typeof(ISchema), typeof(int), typeOfValue);

            var lambda = Expression.Lambda(typeOfFunc, Visit(expression), _schemaParameter, _idParameter);

            var projectedField = Activator.CreateInstance(projectedFieldType, projectedFieldName, lambda.Compile());

            _projectedFields.Add(projectedFieldName, (IField)projectedField);
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
            if(_skip)
            {
                return base.VisitMember(node);
            }

            var oldFieldName = node.Member.Name;
            if (!_oldFieldNamesToNewFieldNamesMapping.TryGetValue(oldFieldName, out ISet<string> newFieldNames))
            {
                newFieldNames = new HashSet<string>();
                _oldFieldNamesToNewFieldNamesMapping.Add(oldFieldName, newFieldNames);
            }
            newFieldNames.Add(_currentProjectedName);

            var genericMethodInfo = _getFieldMethodInfo.MakeGenericMethod(node.Type);
            var fieldAccessExpression = Expression.Call(_schemaParameter, genericMethodInfo, Expression.Constant(oldFieldName, typeof(string)));

            var valueAccessMemberInfo = genericMethodInfo.ReturnType.GetTypeInfo().GetDeclaredMethod("GetValue");

            var valueAccessExpression = Expression.Call(fieldAccessExpression, valueAccessMemberInfo, _idParameter);
            return valueAccessExpression;
        }
    }
}

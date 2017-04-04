using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Projection
{
    public class ProjectionVisitor : ExpressionVisitor
    {
        private ParameterExpression _schemaParameter;
        private ParameterExpression _idParameter;
        private MethodInfo _getFieldMethodInfo;
        private Dictionary<string, IField> _projectedFields;

        public ProjectionVisitor()
        {
            _schemaParameter = Expression.Parameter(typeof(ISchema), "schema");
            _idParameter = Expression.Parameter(typeof(int), "id");

            _getFieldMethodInfo = typeof(ISchema).GetMethod("GetField");
            _projectedFields = new Dictionary<string, IField>();
        }

        public IDictionary<string, IField> GenerateProjection<Tsource, TDest>(Expression<Func<Tsource, TDest>> transformerExpression)
        {
            _projectedFields.Clear();

            Visit(transformerExpression);

            return _projectedFields;
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
            var projectedFieldType = typeof(ProjectedField<>).MakeGenericType(typeOfValue);

            var typeOfFunc = typeof(Func<,,>).MakeGenericType(typeof(ISchema), typeof(int), typeOfValue);

            var lambda = Expression.Lambda(typeOfFunc, Visit(expression), _schemaParameter, _idParameter);

            var projectedField = Activator.CreateInstance(projectedFieldType, projectedFieldName, lambda.Compile());

            _projectedFields.Add(projectedFieldName, (IField)projectedField);
        }

        protected override Expression VisitNew(NewExpression node)
        {
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
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var genericMethodInfo = _getFieldMethodInfo.MakeGenericMethod(node.Type);
            var fieldAccessExpression = Expression.Call(_schemaParameter, genericMethodInfo, _idParameter, Expression.Constant(node.Member.Name, typeof(string)));

            var valueAccessMemberInfo = genericMethodInfo.ReturnType.GetMember("Value")[0];

            var valueAccessExpression = Expression.MakeMemberAccess(fieldAccessExpression, valueAccessMemberInfo);
            return valueAccessExpression;
        }
    }
}

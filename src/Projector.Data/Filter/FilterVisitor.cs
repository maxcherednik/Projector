using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Filter
{
    public class FilterVisitor : ExpressionVisitor
    {
        private ParameterExpression _schemaParameter;
        private ParameterExpression _idParameter;
        private MethodInfo _getFieldMethodInfo;
        private HashSet<string> _usedFieldNames;

        public FilterVisitor()
        {
            _schemaParameter = Expression.Parameter(typeof(ISchema), "schema");
            _idParameter = Expression.Parameter(typeof(int), "id");

            _getFieldMethodInfo = typeof(ISchema).GetTypeInfo().GetDeclaredMethod("GetField");

            _usedFieldNames = new HashSet<string>();
        }
        public Tuple<HashSet<string>, Func<ISchema, int, bool>> GenerateFilter<Tsource>(Expression<Func<Tsource, bool>> filterExpression)
        {
            var newexpression = (Expression<Func<ISchema, int, bool>>)Visit(filterExpression);

            return Tuple.Create(_usedFieldNames, newexpression.Compile());
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _usedFieldNames.Add(node.Member.Name);
            var genericMethodInfo = _getFieldMethodInfo.MakeGenericMethod(node.Type);
            var fieldAccessExpression = Expression.Call(_schemaParameter, genericMethodInfo, Expression.Constant(node.Member.Name, typeof(string)));

            var valueAccessMemberInfo = genericMethodInfo.ReturnType.GetTypeInfo().GetDeclaredMethod("GetValue");

            var valueAccessExpression = Expression.Call(fieldAccessExpression, valueAccessMemberInfo, _idParameter);
            return valueAccessExpression;
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Expression.Lambda<Func<ISchema, int, bool>>(Visit(node.Body), _schemaParameter, _idParameter);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _schemaParameter;
        }
    }
}

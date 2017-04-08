using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Filter
{
    public class FilterVisitor : ExpressionVisitor
    {
        private ParameterExpression _schemaParameter;
        private ParameterExpression _idParameter;
        private MethodInfo _getFieldMethodInfo;

        public FilterVisitor ()
        {
            _schemaParameter = Expression.Parameter (typeof (ISchema), "schema");
            _idParameter = Expression.Parameter (typeof (int), "id");

            _getFieldMethodInfo = typeof (ISchema).GetTypeInfo ().GetDeclaredMethod ("GetField");
        }
        public Func<ISchema, int, bool> GenerateFilter<Tsource> (Expression<Func<Tsource, bool>> filterExpression)
        {
            var newexpression = (Expression<Func<ISchema, int, bool>>)Visit (filterExpression);

            return newexpression.Compile ();
        }

        protected override Expression VisitMember (MemberExpression node)
        {
            var genericMethodInfo = _getFieldMethodInfo.MakeGenericMethod (node.Type);
            var fieldAccessExpression = Expression.Call (_schemaParameter, genericMethodInfo, _idParameter, Expression.Constant (node.Member.Name, typeof (string)));

            var valueAccessMemberInfo = genericMethodInfo.ReturnType.GetTypeInfo ().GetDeclaredProperty ("Value");

            var valueAccessExpression = Expression.MakeMemberAccess (fieldAccessExpression, valueAccessMemberInfo);
            return valueAccessExpression;
        }

        protected override Expression VisitLambda<T> (Expression<T> node)
        {
            return Expression.Lambda<Func<ISchema, int, bool>> (Visit (node.Body), _schemaParameter, _idParameter);
        }

        protected override Expression VisitParameter (ParameterExpression node)
        {
            return _schemaParameter;
        }
    }
}

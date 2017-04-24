using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Join
{
    class ResultSelectorVisitor : ExpressionVisitor
    {
        private string _currentProjectedName;

        private IDictionary<string, IField> _projectedFields;
        private IDictionary<string, ISet<string>> _oldFieldNamesToNewFieldNamesMapping;
        private ParameterExpression _schemaParameter;
        private ParameterExpression _idParameter;
        private object _getFieldMethodInfo;

        public ResultSelectorVisitor()
        {
            _schemaParameter = Expression.Parameter(typeof(ISchema), "schema");
            _idParameter = Expression.Parameter(typeof(int), "id");

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

        private void GenerateField(string projectedFieldName, Type typeOfValue, Expression expression)
        {
            _currentProjectedName = projectedFieldName;
            var projectedFieldType = typeof(JoinProjectedField<>).MakeGenericType(typeOfValue);

            var typeOfFunc = typeof(Func<,,,,>).MakeGenericType(typeof(ISchema), typeof(int), typeof(ISchema), typeof(int), typeOfValue);

            var lambda = Expression.Lambda(typeOfFunc, Visit(expression), _schemaParameter, _idParameter);

            var projectedField = Activator.CreateInstance(projectedFieldType, projectedFieldName, lambda.Compile());

            _projectedFields.Add(projectedFieldName, (IField)projectedField);
        }
    }
}

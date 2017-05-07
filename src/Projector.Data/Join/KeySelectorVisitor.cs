using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Projector.Data.Join
{
    public class KeySelectorVisitor<TLeft, TRight, TKey> : ExpressionVisitor
    {
        private Expression<Func<TLeft, TKey>> _leftKeySelector;
        private Expression<Func<TRight, TKey>> _rightKeySelector;
        private ParameterExpression _schemaParameter;
        private ParameterExpression _idParameter;
        private MethodInfo _getFieldMethodInfo;

        private bool _left;
        private HashSet<string> _leftKeyFieldNames;
        private HashSet<string> _rightKeyFieldNames;

        public KeySelectorVisitor(Expression<Func<TLeft, TKey>> leftKeySelector, Expression<Func<TRight, TKey>> rightKeySelector)
        {
            _leftKeySelector = leftKeySelector;
            _rightKeySelector = rightKeySelector;

            _schemaParameter = Expression.Parameter(typeof(ISchema), "schema");
            _idParameter = Expression.Parameter(typeof(int), "id");

            _getFieldMethodInfo = typeof(ISchema).GetTypeInfo().GetDeclaredMethod("GetField");

            _leftKeyFieldNames = new HashSet<string>();
            _rightKeyFieldNames = new HashSet<string>();
        }

        public KeyFieldsMeta Generate()
        {
            var keyFieldMtchers = new List<IKeyFieldMatcher>();

            _left = true;

            var projectedKeyFieldsLeft = GenerateFieldAccessor(_leftKeySelector);

            _left = false;

            var projectedKeyFieldsRight = GenerateFieldAccessor(_rightKeySelector);

            for (int i = 0; i < projectedKeyFieldsLeft.Count; i++)
            {
                var projectedKeyFieldLeft = projectedKeyFieldsLeft[i];
                var projectedKeyFieldRight = projectedKeyFieldsRight[i];

                var keyFieldType = typeof(KeyFieldMatcher<>).MakeGenericType(projectedKeyFieldLeft.ReturnType);

                var fieldMatcherInstance = (IKeyFieldMatcher)Activator.CreateInstance(keyFieldType, projectedKeyFieldLeft.Compile(), projectedKeyFieldRight.Compile());
                keyFieldMtchers.Add(fieldMatcherInstance);
            }

            var keyFieldsMeta = new KeyFieldsMeta(_leftKeyFieldNames, _rightKeyFieldNames,
                                 (ISchema leftSchema, int leftRowid, ISchema rightSchema, int rightRowid) =>
                                 {
                                     foreach (var keyFieldMatcher in keyFieldMtchers)
                                     {
                                         if (!keyFieldMatcher.Match(leftSchema, leftRowid, rightSchema, rightRowid))
                                         {
                                             return false;
                                         }
                                     }

                                     return true;
                                 });

            return keyFieldsMeta;
        }

        private List<LambdaExpression> GenerateFieldAccessor<T1, T2>(Expression<Func<T1, T2>> keySelecter)
        {
            if (keySelecter.Body.NodeType == ExpressionType.New)
            {
                return GenerateFieldAccessorForAnonymousType((NewExpression)keySelecter.Body);
            }
            else if (keySelecter.Body.NodeType == ExpressionType.MemberInit)
            {
                return GenerateFieldAccessorForConcreteType((MemberInitExpression)keySelecter.Body);
            }
            else
            {
                var node = keySelecter.Body;
                var typeOfvalue = node.Type;

                return new List<LambdaExpression>
                {
                     GenerateField(typeOfvalue, node)
                };
            }
        }

        private LambdaExpression GenerateField(Type typeOfValue, Expression expression)
        {
            var typeOfFunc = typeof(Func<,,>).MakeGenericType(typeof(ISchema), typeof(int), typeOfValue);

            return Expression.Lambda(typeOfFunc, Visit(expression), _schemaParameter, _idParameter);
        }

        private List<LambdaExpression> GenerateFieldAccessorForAnonymousType(NewExpression node)
        {
            var projectedKeyFields = new List<LambdaExpression>();

            for (int i = 0; i < node.Members.Count; i++)
            {
                var member = node.Members[i];
                var valueExpression = node.Arguments[i];
                var typeOfvalue = valueExpression.Type;

                projectedKeyFields.Add(GenerateField(typeOfvalue, valueExpression));
            }


            return projectedKeyFields;
        }

        private List<LambdaExpression> GenerateFieldAccessorForConcreteType(MemberInitExpression node)
        {
            var projectedKeyFields = new List<LambdaExpression>();

            foreach (var nodeBinding in node.Bindings)
            {
                var member = (MemberAssignment)nodeBinding;
                var valueExpression = member.Expression;
                var typeOfvalue = valueExpression.Type;

                projectedKeyFields.Add(GenerateField(typeOfvalue, valueExpression));
            }

            return projectedKeyFields;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var oldFieldName = node.Member.Name;

            if (_left)
            {
                _leftKeyFieldNames.Add(oldFieldName);
            }
            else
            {
                _rightKeyFieldNames.Add(oldFieldName);
            }


            var genericMethodInfo = _getFieldMethodInfo.MakeGenericMethod(node.Type);
            var fieldAccessExpression = Expression.Call(_schemaParameter, genericMethodInfo, Expression.Constant(oldFieldName, typeof(string)));

            var valueAccessMemberInfo = genericMethodInfo.ReturnType.GetTypeInfo().GetDeclaredMethod("GetValue");

            var valueAccessExpression = Expression.Call(fieldAccessExpression, valueAccessMemberInfo, _idParameter);
            return valueAccessExpression;
        }
    }
}
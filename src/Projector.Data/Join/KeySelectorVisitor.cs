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

        public KeySelectorVisitor(Expression<Func<TLeft, TKey>> leftKeySelector, Expression<Func<TRight, TKey>> rightKeySelector)
        {
            _leftKeySelector = leftKeySelector;
            _rightKeySelector = rightKeySelector;

            _schemaParameter = Expression.Parameter(typeof(ISchema), "schema");
            _idParameter = Expression.Parameter(typeof(int), "id");

            _getFieldMethodInfo = typeof(ISchema).GetTypeInfo().GetDeclaredMethod("GetField");
        }

        public KeyFieldsMeta Generate()
        {
            var keyFieldMtchers = new List<IKeyFieldMatcher>();

            var projectedKeyFieldsLeft = GenerateFieldAccessor(_leftKeySelector);

            var projectedKeyFieldsRight = GenerateFieldAccessor(_rightKeySelector);

            var leftKeyFieldNames = new HashSet<string>();
            var rightKeyFieldNames = new HashSet<string>();

            for (int i = 0; i < projectedKeyFieldsLeft.Count; i++)
            {
                var projectedKeyFieldLeft = projectedKeyFieldsLeft[i];
                var projectedKeyFieldRight = projectedKeyFieldsRight[i];

                leftKeyFieldNames.Add(projectedKeyFieldLeft.Item1);
                rightKeyFieldNames.Add(projectedKeyFieldRight.Item1);

                var keyFieldType = typeof(KeyFieldMatcher<>).MakeGenericType(projectedKeyFieldLeft.Item2.ReturnType);

                var fieldMatcherInstance = (IKeyFieldMatcher)Activator.CreateInstance(keyFieldType, projectedKeyFieldLeft.Item2.Compile(), projectedKeyFieldRight.Item2.Compile());
                keyFieldMtchers.Add(fieldMatcherInstance);
            }

            var keyFieldsMeta = new KeyFieldsMeta(leftKeyFieldNames, rightKeyFieldNames,
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

        private List<Tuple<string, LambdaExpression>> GenerateFieldAccessor<T1, T2>(Expression<Func<T1, T2>> keySelecter)
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
                var projectedFieldName = "";
                var typeOfvalue = node.Type;

                return new List<Tuple<string, LambdaExpression>>
                {
                    Tuple.Create(projectedFieldName, GenerateField(typeOfvalue, node))
                };
            }
        }

        private LambdaExpression GenerateField(Type typeOfValue, Expression expression)
        {
            var typeOfFunc = typeof(Func<,,>).MakeGenericType(typeof(ISchema), typeof(int), typeOfValue);

            return Expression.Lambda(typeOfFunc, Visit(expression), _schemaParameter, _idParameter);
        }

        private List<Tuple<string, LambdaExpression>> GenerateFieldAccessorForAnonymousType(NewExpression node)
        {
            var projectedKeyFields = new List<Tuple<string, LambdaExpression>>();
            // this is for anonymous types only
            if (node.Members != null)
            {
                for (int i = 0; i < node.Members.Count; i++)
                {
                    var member = node.Members[i];
                    var valueExpression = node.Arguments[i];
                    var projectedFieldName = member.Name;
                    var typeOfvalue = valueExpression.Type;

                    projectedKeyFields.Add(Tuple.Create(projectedFieldName, GenerateField(typeOfvalue, valueExpression)));
                }
            }

            return projectedKeyFields;
        }

        private List<Tuple<string, LambdaExpression>> GenerateFieldAccessorForConcreteType(MemberInitExpression node)
        {
            var projectedKeyFields = new List<Tuple<string, LambdaExpression>>();

            foreach (var nodeBinding in node.Bindings)
            {
                var member = (MemberAssignment)nodeBinding;
                var valueExpression = member.Expression;
                var projectedFieldName = member.Member.Name;
                var typeOfvalue = valueExpression.Type;

                projectedKeyFields.Add(Tuple.Create(projectedFieldName, GenerateField(typeOfvalue, valueExpression)));
            }

            return projectedKeyFields;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var oldFieldName = node.Member.Name;

            var genericMethodInfo = _getFieldMethodInfo.MakeGenericMethod(node.Type);
            var fieldAccessExpression = Expression.Call(_schemaParameter, genericMethodInfo, Expression.Constant(oldFieldName, typeof(string)));

            var valueAccessMemberInfo = genericMethodInfo.ReturnType.GetTypeInfo().GetDeclaredMethod("GetValue");

            var valueAccessExpression = Expression.Call(fieldAccessExpression, valueAccessMemberInfo, _idParameter);
            return valueAccessExpression;
        }
    }
}
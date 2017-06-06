using System;
using System.Linq.Expressions;

namespace Projector.Data.Join
{
    public static class JoinExtensions
    {
        public static Join<TLeft, TRight, TKey, TResult> InnerJoin<TLeft, TRight, TKey, TResult>(this IDataProvider<TLeft> leftSource,
                                                                                        IDataProvider<TRight> rightSource,
                                                                                        Expression<Func<TLeft, TKey>> leftKeySelector,
                                                                                        Expression<Func<TRight, TKey>> rightKeySelector,
                                                                                        Expression<Func<TLeft, TRight, TResult>> resultColumnsSelector)
        {
            return new Join<TLeft, TRight, TKey, TResult>(leftSource, rightSource, leftKeySelector, rightKeySelector, resultColumnsSelector);
        }
    }
}

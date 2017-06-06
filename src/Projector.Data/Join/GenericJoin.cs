using System;
using System.Linq.Expressions;

namespace Projector.Data.Join
{
    public class Join<TLeft, TRight, TKey, TResult> : Join, IDataProvider<TResult>
    {
        public Join(IDataProvider<TLeft> leftSource,
                    IDataProvider<TRight> rightSource,
                    Expression<Func<TLeft, TKey>> leftKeySelector,
                    Expression<Func<TRight, TKey>> rightKeySelector,
                    Expression<Func<TLeft, TRight, TResult>> resultColumnsSelector) :
            base(leftSource,
                rightSource,
                new KeySelectorVisitor<TLeft, TRight, TKey>(leftKeySelector, rightKeySelector).Generate(),
                new ResultSelectorVisitor().GenerateProjection(resultColumnsSelector))
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Projector.Data.GroupBy
{
    public static class GroupByExtensions
    {
        public static GroupBy<TSource, TDest> GroupBy<TSource, TKey, TDest> (this IDataProvider<TSource> source,
                                                    Expression<Func<TSource, TKey>> keySelector,
                                                    Expression<Func<TKey, IEnumerable<TSource>, TDest>> resultSelector)
        {
            return new GroupBy<TSource, TDest> (source);
        }
    }
}

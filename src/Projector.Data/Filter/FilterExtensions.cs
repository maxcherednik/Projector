using System;
using System.Linq.Expressions;

namespace Projector.Data.Filter
{
    public static class FilterExtensions
    {
        public static Filter<TSource> Where<TSource> (this IDataProvider<TSource> source, Expression<Func<TSource, bool>> filterExpression)
        {
            return new Filter<TSource> (source, filterExpression);
        }
    }
}

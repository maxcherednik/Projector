using System;
using System.Linq.Expressions;

namespace Projector.Data.Filter
{
    public static class FilterExtensions
    {
        public static Filter<Tsource> Where<Tsource> (this IDataProvider<Tsource> source, Expression<Func<Tsource, bool>> filterExpression)
        {
            return new Filter<Tsource> (source, filterExpression);
        }
    }
}

using System;
using System.Linq.Expressions;

namespace Projector.Data.Filter
{
    public class Filter<T> : Filter, IDataProvider<T>
    {
        public Filter(IDataProvider<T> sourceDataProvider, Expression<Func<T, bool>> filterExpression)
            : base(sourceDataProvider, new FilterVisitor().GenerateFilter(filterExpression))
        {
            
        }

        public void ChangeFilter(Expression<Func<T, bool>> filterExpression)
        {
            var filter = new FilterVisitor().GenerateFilter(filterExpression);
            ChangeFilter(filter);
        }
    }
}

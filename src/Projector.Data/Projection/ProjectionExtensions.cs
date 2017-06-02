using System;
using System.Linq.Expressions;

namespace Projector.Data.Projection
{
    public static class ProjectionExtensions
    {
        public static Projection<TSource, TDest> Select<TSource, TDest> (this IDataProvider<TSource> source, Expression<Func<TSource, TDest>> transformerExpression)
        {
            return new Projection<TSource, TDest> (source, transformerExpression);
        }
    }
}

using System;
using System.Linq.Expressions;

namespace Projector.Data.Projection
{
    public static class ProjectionExtensions
    {
        public static Projection<Tsource, TDest> Select<Tsource, TDest> (this IDataProvider<Tsource> source, Expression<Func<Tsource, TDest>> transformerExpression)
        {
            return new Projection<Tsource, TDest> (source, transformerExpression);
        }
    }
}

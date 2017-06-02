using System;
using System.Linq.Expressions;

namespace Projector.Data.Projection
{
    public class Projection<TSource, TDest> : Projection, IDataProvider<TDest>
    {
        public Projection(IDataProvider<TSource> sourceDataProvider, Expression<Func<TSource, TDest>> transformerExpression)
            : base(sourceDataProvider, new ProjectionVisitor().GenerateProjection(transformerExpression))
        {

        }
    }
}

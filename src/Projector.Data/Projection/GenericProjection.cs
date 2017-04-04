using System;
using System.Linq.Expressions;

namespace Projector.Data.Projection
{
    public class Projection<Tsource, TDest> : Projection, IDataProvider<TDest>
    {
        public Projection(IDataProvider<Tsource> sourceDataProvider, Expression<Func<Tsource, TDest>> transformerExpression)
            : base(sourceDataProvider, new ProjectionVisitor().GenerateProjection(transformerExpression))
        {

        }
    }
}

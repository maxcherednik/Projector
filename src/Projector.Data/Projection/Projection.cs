using System;
using System.Collections.Generic;

namespace Projector.Data.Projection
{
    public class Projection : DataProviderBase, IDataConsumer
    {
        private IDictionary<string, IField> _projectionFields;
        private IDisconnectable _subscription;

        public Projection(IDataProvider sourceDataProvider, IDictionary<string, IField> projectionFields)
        {
            _projectionFields = projectionFields;
            _subscription = sourceDataProvider.AddConsumer(this);
        }

        public void OnSchema(ISchema schema)
        {

            var projectedSchema = new ProjectionSchema(schema, _projectionFields);
            SetSchema(projectedSchema);
        }

        public void OnSyncPoint()
        {
            base.FireChanges();
        }

        public void OnAdd(IList<int> ids)
        {
            foreach (var id in ids)
            {
                AddId(id);
            }
        }

        public void OnUpdate(IList<int> ids, IList<IField> updatedFields)
        {
            throw new NotImplementedException();
        }

        public void OnDelete(IList<int> ids)
        {
            foreach (var id in ids)
            {
                RemoveId(id);
            }
        }


    }
}

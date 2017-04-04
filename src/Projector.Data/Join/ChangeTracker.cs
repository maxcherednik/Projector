using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    class ChangeTracker : IDataConsumer
    {
        private IDisconnectable _subscription;

        public void SetSource(IDataProvider sourceDataProvider)
        {
            _subscription = sourceDataProvider.AddConsumer(this);
        }

        public void OnAdd(IList<int> ids)
        {
            FireOnAdd(ids);
        }

        public void OnUpdate(IList<int> ids, IList<IField> updatedFields)
        {
            FireOnUpdate(ids, updatedFields);
        }

        public void OnDelete(IList<int> ids)
        {
            FireOnDelete(ids);
        }

        public void OnSchema(ISchema schema)
        {
            FireOnSchema(schema);
        }

        public void OnSyncPoint()
        {
            FireOnSyncPoint();
        }

        public event Action<IList<int>> OnAdded;
        public event Action<IList<int>, IList<IField>> OnUpdated;
        public event Action<IList<int>> OnDeleted;
        public event Action<ISchema> OnSchemaArrived;
        public event Action OnSyncPointArrived;

        private void FireOnAdd(IList<int> ids)
        {
            var handler = OnAdded;
            if (handler != null)
            {
                handler(ids);
            }
        }

        private void FireOnDelete(IList<int> ids)
        {
            var handler = OnDeleted;
            if (handler != null)
            {
                handler(ids);
            }
        }

        private void FireOnUpdate(IList<int> ids, IList<IField> fields)
        {
            var handler = OnUpdated;
            if (handler != null)
            {
                handler(ids, fields);
            }
        }

        private void FireOnSchema(ISchema schema)
        {
            var handler = OnSchemaArrived;
            if (handler != null)
            {
                handler(schema);
            }
        }

        private void FireOnSyncPoint()
        {
            var handler = OnSyncPointArrived;
            if (handler != null)
            {
                handler();
            }
        }
    }
}

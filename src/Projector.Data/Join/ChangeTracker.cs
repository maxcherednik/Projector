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

        public void OnAdd(IReadOnlyCollection<int> ids)
        {
            FireOnAdd(ids);
        }

        public void OnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            FireOnUpdate(ids, updatedFields);
        }

        public void OnDelete(IReadOnlyCollection<int> ids)
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

        public event Action<IReadOnlyCollection<int>> OnAdded;
        public event Action<IReadOnlyCollection<int>, IReadOnlyCollection<IField>> OnUpdated;
        public event Action<IReadOnlyCollection<int>> OnDeleted;
        public event Action<ISchema> OnSchemaArrived;
        public event Action OnSyncPointArrived;

        private void FireOnAdd(IReadOnlyCollection<int> ids)
        {
            var handler = OnAdded;
            if (handler != null)
            {
                handler(ids);
            }
        }

        private void FireOnDelete(IReadOnlyCollection<int> ids)
        {
            var handler = OnDeleted;
            if (handler != null)
            {
                handler(ids);
            }
        }

        private void FireOnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> fields)
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

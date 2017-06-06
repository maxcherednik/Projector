using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    internal class ChangeTracker : IDataConsumer, IDisposable
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
            OnAdded?.Invoke(ids);
        }

        private void FireOnDelete(IReadOnlyCollection<int> ids)
        {
            OnDeleted?.Invoke(ids);
        }

        private void FireOnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> fields)
        {
            OnUpdated?.Invoke(ids, fields);
        }

        private void FireOnSchema(ISchema schema)
        {
            OnSchemaArrived?.Invoke(schema);
        }

        private void FireOnSyncPoint()
        {
            OnSyncPointArrived?.Invoke();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;

namespace Projector.Data
{
    public class DataProviderBase : IDataProvider
    {
        private ISchema _schema;

        protected HashSet<int> UsedIds;

        protected HashSet<int> CurrentAddedIds;
        protected HashSet<int> CurrentUpdatedIds;
        protected HashSet<int> CurrentRemovedIds;

        private HashSet<IField> _updatedFields;

        public DataProviderBase()
        {
            UsedIds = new HashSet<int>();
            CurrentAddedIds = new HashSet<int>();
            CurrentUpdatedIds = new HashSet<int>();
            CurrentRemovedIds = new HashSet<int>();
            _updatedFields = new HashSet<IField>();
        }

        protected void SetSchema(ISchema schema)
        {
            _schema = schema;
        }

        protected void AddId(int id)
        {
            if (!CurrentAddedIds.Add(id))
            {
                throw new InvalidOperationException("Duplicate key in the 'add'' collection");
            }
        }

        protected void UpdateId(int id, IField field)
        {
            CurrentUpdatedIds.Add(id);
            _updatedFields.Add(field);
        }

        protected void RemoveId(int id)
        {
            if (!CurrentRemovedIds.Add(id))
            {
                throw new InvalidOperationException("Duplicate key in the 'remove' collection");
            }
        }

        protected void FireChanges()
        {
            var thereWereChanges = false;
            foreach (var newId in CurrentAddedIds)
            {
                UsedIds.Add(newId);
            }

            foreach (var removeId in CurrentRemovedIds)
            {
                UsedIds.Remove(removeId);
            }

            if (CurrentRemovedIds.Count > 0)
            {
                thereWereChanges = true;
                FireOnDelete((IReadOnlyCollection<int>)CurrentRemovedIds);
                CurrentRemovedIds.Clear();
            }

            if (CurrentAddedIds.Count > 0)
            {
                thereWereChanges = true;
                FireOnAdd((IReadOnlyCollection<int>)CurrentAddedIds);
                CurrentAddedIds.Clear();
            }

            if (CurrentUpdatedIds.Count > 0)
            {
                thereWereChanges = true;
                FireOnUpdate((IReadOnlyCollection<int>)CurrentUpdatedIds, (IReadOnlyCollection<IField>)_updatedFields);
                CurrentUpdatedIds.Clear();
                _updatedFields.Clear();
            }

            if (thereWereChanges)
            {
                FireOnSyncPoint();
            }
        }

        public IDisconnectable AddConsumer(IDataConsumer consumer)
        {
            consumer.OnSchema(_schema);

            if (UsedIds.Count > 0)
            {
                consumer.OnAdd(UsedIds.ToList());
            }

            consumer.OnSyncPoint();

            OnAdd += consumer.OnAdd;
            OnDelete += consumer.OnDelete;
            OnUpdate += consumer.OnUpdate;
            OnSchema += consumer.OnSchema;
            OnSyncPoint += consumer.OnSyncPoint;

            return new Disconnectable(this, consumer);
        }

        public void RemoveConsumer(IDataConsumer consumer)
        {
            OnAdd -= consumer.OnAdd;
            OnDelete -= consumer.OnDelete;
            OnUpdate -= consumer.OnUpdate;
            OnSchema -= consumer.OnSchema;
            OnSyncPoint -= consumer.OnSyncPoint;
        }

        public ISchema Schema { get { return _schema; } }

        private event Action<IReadOnlyCollection<int>> OnAdd;
        private event Action<IReadOnlyCollection<int>, IReadOnlyCollection<IField>> OnUpdate;
        private event Action<IReadOnlyCollection<int>> OnDelete;
        private event Action<ISchema> OnSchema;
        private event Action OnSyncPoint;

        private void FireOnAdd(IReadOnlyCollection<int> ids)
        {
            var handler = OnAdd;
            if (handler != null)
            {
                handler(ids);
            }
        }

        private void FireOnDelete(IReadOnlyCollection<int> ids)
        {
            var handler = OnDelete;
            if (handler != null)
            {
                handler(ids);
            }
        }

        private void FireOnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> fields)
        {
            var handler = OnUpdate;
            if (handler != null)
            {
                handler(ids, fields);
            }
        }

        private void FireOnSchema(ISchema schema)
        {
            var handler = OnSchema;
            if (handler != null)
            {
                handler(schema);
            }
        }

        private void FireOnSyncPoint()
        {
            var handler = OnSyncPoint;
            if (handler != null)
            {
                handler();
            }
        }
    }
}

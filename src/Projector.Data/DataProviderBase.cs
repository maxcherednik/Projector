﻿using System;
using System.Collections.Generic;

namespace Projector.Data
{
    public class DataProviderBase : IDataProvider
    {
        protected HashSet<int> CurrentAddedIds;
        protected HashSet<int> CurrentUpdatedIds;
        protected HashSet<int> CurrentRemovedIds;

        private readonly HashSet<IField> _updatedFields;

        private readonly ReadOnlyCollectionWrapper<int> _currentAddedIdsReadOnlyCollection;
        private readonly ReadOnlyCollectionWrapper<int> _currentUpdatedIdsReadOnlyCollection;
        private readonly ReadOnlyCollectionWrapper<int> _currentRemovedIdsReadOnlyCollection;
        private readonly ReadOnlyCollectionWrapper<IField> _updatedFieldsReadOnlyCollection;

        public DataProviderBase()
        {
            CurrentAddedIds = new HashSet<int>();
            CurrentUpdatedIds = new HashSet<int>();
            CurrentRemovedIds = new HashSet<int>();
            _updatedFields = new HashSet<IField>();

            _currentAddedIdsReadOnlyCollection = new ReadOnlyCollectionWrapper<int>(CurrentAddedIds);
            _currentUpdatedIdsReadOnlyCollection = new ReadOnlyCollectionWrapper<int>(CurrentUpdatedIds);
            _currentRemovedIdsReadOnlyCollection = new ReadOnlyCollectionWrapper<int>(CurrentRemovedIds);
            _updatedFieldsReadOnlyCollection = new ReadOnlyCollectionWrapper<IField>(_updatedFields);

        }

        protected void SetRowIds(IReadOnlyCollection<int> usedRowIds)
        {
            RowIds = usedRowIds;
        }

        protected void SetSchema(ISchema schema)
        {
            Schema = schema;
        }

        protected void AddId(int id)
        {
            if (!CurrentAddedIds.Add(id))
            {
                throw new InvalidOperationException("Duplicate key in the 'add' collection");
            }
        }

        protected void UpdateId(int id)
        {
            CurrentUpdatedIds.Add(id);
        }

        protected void AddUpdatedField(IField field)
        {
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

            if (CurrentRemovedIds.Count > 0)
            {
                thereWereChanges = true;
                FireOnDelete(_currentRemovedIdsReadOnlyCollection);
                CurrentRemovedIds.Clear();
            }

            if (CurrentAddedIds.Count > 0)
            {
                thereWereChanges = true;
                FireOnAdd(_currentAddedIdsReadOnlyCollection);
                CurrentAddedIds.Clear();
            }

            if (CurrentUpdatedIds.Count > 0)
            {
                thereWereChanges = true;
                FireOnUpdate(_currentUpdatedIdsReadOnlyCollection, _updatedFieldsReadOnlyCollection);
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
            OnAdd += consumer.OnAdd;
            OnDelete += consumer.OnDelete;
            OnUpdate += consumer.OnUpdate;
            OnSchema += consumer.OnSchema;
            OnSyncPoint += consumer.OnSyncPoint;

            FireOnSchema(Schema);

            if (RowIds.Count > 0)
            {
                consumer.OnAdd(RowIds);
            }

            consumer.OnSyncPoint();

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

        public ISchema Schema { get; private set; }

        public IReadOnlyCollection<int> RowIds { get; private set; }

        private event Action<IReadOnlyCollection<int>> OnAdd;
        private event Action<IReadOnlyCollection<int>, IReadOnlyCollection<IField>> OnUpdate;
        private event Action<IReadOnlyCollection<int>> OnDelete;
        private event Action<ISchema> OnSchema;
        private event Action OnSyncPoint;

        private void FireOnAdd(IReadOnlyCollection<int> ids)
        {
            OnAdd?.Invoke(ids);
        }

        private void FireOnDelete(IReadOnlyCollection<int> ids)
        {
            OnDelete?.Invoke(ids);
        }

        private void FireOnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> fields)
        {
            OnUpdate?.Invoke(ids, fields);
        }

        private void FireOnSchema(ISchema schema)
        {
            OnSchema?.Invoke(schema);
        }

        private void FireOnSyncPoint()
        {
            OnSyncPoint?.Invoke();
        }
    }
}

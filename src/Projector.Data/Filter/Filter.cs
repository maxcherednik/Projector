using System;
using System.Collections.Generic;

namespace Projector.Data.Filter
{
    public class Filter : DataProviderBase, IDataConsumer
    {
        private Func<ISchema, int, bool> _filterCriteria;

        private IDisconnectable _subscription;
        private IDataProvider _sourceDataProvider;

        public Filter(IDataProvider sourceDataProvider, Func<ISchema, int, bool> filterCriteria)
        {
            _filterCriteria = filterCriteria;
            _sourceDataProvider = sourceDataProvider;
            _subscription = sourceDataProvider.AddConsumer(this);
        }

        public void ChangeFilter(Func<ISchema, int, bool> filterCriteria)
        {
            _filterCriteria = filterCriteria;
            _subscription.Dispose();

            foreach (var id in UsedIds)
            {
                RemoveId(id);
            }

            _subscription = _sourceDataProvider.AddConsumer(this);
            FireChanges();
        }

        public void OnSchema(ISchema schema)
        {
            SetSchema(schema);
        }

        public void OnSyncPoint()
        {
            FireChanges();
        }

        public void OnAdd(IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids)
            {
                if (_filterCriteria(Schema, id))
                {
                    AddId(id);
                }
            }
        }

        public void OnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            foreach (var id in ids)
            {
                if (UsedIds.Contains(id) && !_filterCriteria(Schema, id))
                {
                    RemoveId(id);
                }
                else if (!UsedIds.Contains(id) && _filterCriteria(Schema, id))
                {
                    AddId(id);
                }
                else
                {
                    foreach (var updatedField in updatedFields)
                    {
                        UpdateId(id, updatedField);
                    }
                }
            }
        }

        public void OnDelete(IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids)
            {
                if (UsedIds.Contains(id))
                {
                    RemoveId(id);
                }
            }
        }
    }
}

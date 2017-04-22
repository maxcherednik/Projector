using System;
using System.Collections.Generic;

namespace Projector.Data.Filter
{
    public class Filter : DataProviderBase, IDataConsumer
    {
        private Func<ISchema, int, bool> _filterCriteria;

        private HashSet<string> _fieldsUsedInFilter;

        private IDisconnectable _subscription;
        private IDataProvider _sourceDataProvider;

        public Filter(IDataProvider sourceDataProvider, Tuple<HashSet<string>, Func<ISchema, int, bool>> filterCriteriaMeta)
        {
            _fieldsUsedInFilter = filterCriteriaMeta.Item1;
            _filterCriteria = filterCriteriaMeta.Item2;
            _sourceDataProvider = sourceDataProvider;
            _subscription = sourceDataProvider.AddConsumer(this);
        }

        public void ChangeFilter(Tuple<HashSet<string>, Func<ISchema, int, bool>> filterCriteriaMeta)
        {
            _fieldsUsedInFilter = filterCriteriaMeta.Item1;
            _filterCriteria = filterCriteriaMeta.Item2;
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
            // if fields, which are used in the filter, updated, we need to to recheck filter criteria
            if (FieldsInUse(updatedFields))
            {
                foreach (var id in ids)
                {
                    var usedBefore = UsedIds.Contains(id);
                    var usedAfter = _filterCriteria(Schema, id);

                    if (usedBefore && !usedAfter)
                    {
                        RemoveId(id);
                    }
                    else if (!usedBefore && usedAfter)
                    {
                        AddId(id);
                    }
                    else if (usedBefore && usedAfter)
                    {
                        foreach (var updatedField in updatedFields)
                        {
                            UpdateId(id, updatedField);
                        }
                    }
                }
            }
            else
            {
                // if fields, which are not used in the filter, updated, we need to propagate set of ids,
                // which were checked before without rechecking filter criteria
                foreach (var id in ids)
                {
                    if (UsedIds.Contains(id))
                    {
                        foreach (var updatedField in updatedFields)
                        {
                            UpdateId(id, updatedField);
                        }
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

        private bool FieldsInUse(IReadOnlyCollection<IField> updatedFields)
        {
            foreach (var updatedField in updatedFields)
            {
                if (_fieldsUsedInFilter.Contains(updatedField.Name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

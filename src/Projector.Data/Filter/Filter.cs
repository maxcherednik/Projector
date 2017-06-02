using System;
using System.Collections.Generic;

namespace Projector.Data.Filter
{
    public class Filter : DataProviderBase, IDataConsumer
    {
        private Func<ISchema, int, bool> _filterCriteria;

        private HashSet<string> _fieldsUsedInFilter;

        private IDisconnectable _subscription;

        private readonly HashSet<int> _usedRowIds;

        private readonly IReadOnlyCollection<int> _parentRowIds;

        public Filter(IDataProvider sourceDataProvider, Tuple<HashSet<string>, Func<ISchema, int, bool>> filterCriteriaMeta)
        {
            _parentRowIds = sourceDataProvider.RowIds;
            _usedRowIds = new HashSet<int>();

            SetRowIds((IReadOnlyCollection<int>)_usedRowIds);

            _fieldsUsedInFilter = filterCriteriaMeta.Item1;
            _filterCriteria = filterCriteriaMeta.Item2;
            _subscription = sourceDataProvider.AddConsumer(this);
        }

        public void ChangeFilter(Tuple<HashSet<string>, Func<ISchema, int, bool>> filterCriteriaMeta)
        {
            _fieldsUsedInFilter = filterCriteriaMeta.Item1;
            _filterCriteria = filterCriteriaMeta.Item2;

            foreach (var id in _parentRowIds)
            {
                var usedBefore = _usedRowIds.Contains(id);
                var usedAfter = _filterCriteria(Schema, id);

                if (usedBefore && !usedAfter)
                {
                    _usedRowIds.Remove(id);
                    RemoveId(id);
                }
                else if (!usedBefore && usedAfter)
                {
                    _usedRowIds.Add(id);
                    AddId(id);
                }
            }

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
                    _usedRowIds.Add(id);
                    AddId(id);
                }
            }
        }

        public void OnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            var wasUpdated = false;

            // if fields, which are used in the filter, updated, we need to to recheck filter criteria
            if (FieldsInUse(updatedFields))
            {
                foreach (var id in ids)
                {
                    var usedBefore = _usedRowIds.Contains(id);
                    var usedAfter = _filterCriteria(Schema, id);

                    if (usedBefore && !usedAfter)
                    {
                        _usedRowIds.Remove(id);
                        RemoveId(id);
                    }
                    else if (!usedBefore && usedAfter)
                    {
                        _usedRowIds.Add(id);
                        AddId(id);
                    }
                    else if (usedBefore)
                    {
                        UpdateId(id);
                        wasUpdated = true;
                    }
                }
            }
            else
            {
                // if fields, which are not used in the filter, updated, we need to propagate set of ids,
                // which were checked before without rechecking filter criteria
                foreach (var id in ids)
                {
                    if (_usedRowIds.Contains(id))
                    {
                        UpdateId(id);
                        wasUpdated = true;
                    }
                }
            }

            if (wasUpdated)
            {
                foreach (var updatedField in updatedFields)
                {
                    AddUpdatedField(updatedField);
                }
            }
        }

        public void OnDelete(IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids)
            {
                if (_usedRowIds.Contains(id))
                {
                    _usedRowIds.Remove(id);
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

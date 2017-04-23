using System;
using System.Collections.Generic;
using System.Linq;

namespace Projector.Data.Join
{
    public class Join : DataProviderBase, IDataProvider
    {
        private readonly ChangeTracker _leftChangeTracker;
        private readonly ChangeTracker _rightChangeTracker;

        private ISchema _leftSchema;
        private ISchema _rightSchema;

        private readonly HashSet<int> _allLeftRowIds;
        private readonly HashSet<int> _allRightRowIds;

        private Func<ISchema, ISchema, int, int, bool> _rowMatcher;

        private readonly Dictionary<int, int> _leftRowIdToJoinedRowIdMapping;
        private readonly Dictionary<int, int> _rightRowIdToJoinedRowIdMapping;

        private int _latestJoinedRowId = -1;

        private readonly HashSet<int> _freeRows;

        public Join(IDataProvider leftSource,
                    IDataProvider rightSource,
                    JoinType joinType,
                    IDictionary<string, IField> projectionFields)
        {
            _freeRows = new HashSet<int>();

            _allLeftRowIds = new HashSet<int>();
            _leftRowIdToJoinedRowIdMapping = new Dictionary<int, int>();
            _leftChangeTracker = new ChangeTracker();
            _leftChangeTracker.OnAdded += _leftChangeTracker_OnAdded;
            _leftChangeTracker.OnDeleted += _leftChangeTracker_OnDeleted;
            _leftChangeTracker.OnUpdated += _leftChangeTracker_OnUpdated;
            _leftChangeTracker.OnSchemaArrived += _leftChangeTracker_OnSchemaArrived;
            _leftChangeTracker.OnSyncPointArrived += _leftChangeTracker_OnSyncPointArrived;
            _leftChangeTracker.SetSource(leftSource);


            _allRightRowIds = new HashSet<int>();
            _rightRowIdToJoinedRowIdMapping = new Dictionary<int, int>();
            _rightChangeTracker = new ChangeTracker();
            _rightChangeTracker.OnAdded += _rightChangeTracker_OnAdded;
            _rightChangeTracker.OnDeleted += _rightChangeTracker_OnDeleted;
            _rightChangeTracker.OnUpdated += _rightChangeTracker_OnUpdated;
            _rightChangeTracker.OnSchemaArrived += _rightChangeTracker_OnSchemaArrived;
            _rightChangeTracker.OnSyncPointArrived += _rightChangeTracker_OnSyncPointArrived;
            _rightChangeTracker.SetSource(rightSource);
        }

        void _leftChangeTracker_OnSyncPointArrived()
        {
            FireChanges();
        }

        void _leftChangeTracker_OnSchemaArrived(ISchema schema)
        {
            _leftSchema = schema;
        }

        void _leftChangeTracker_OnUpdated(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            throw new System.NotImplementedException();
        }

        void _leftChangeTracker_OnDeleted(IReadOnlyCollection<int> ids)
        {
            ProcessOnDelete(ids, true);
        }

        void _leftChangeTracker_OnAdded(IReadOnlyCollection<int> ids)
        {
            ProcessOnAdd(ids, _allLeftRowIds, (IReadOnlyCollection<int>)_allRightRowIds, _leftSchema, _rightSchema);
        }

        private void ProcessOnAdd(IReadOnlyCollection<int> ids, ISet<int> allLeftIds, IReadOnlyCollection<int> outerIds, ISchema leftSchema, ISchema rightSchema)
        {
            foreach (var id in ids)
            {
                allLeftIds.Add(id);
                foreach (var outerRowId in outerIds)
                {
                    if (_rowMatcher(leftSchema, rightSchema, id, outerRowId))
                    {
                        var joinedRowId = CreateMapping(id, outerRowId);
                        AddId(joinedRowId);
                    }
                }
            }
        }

        private void ProcessOnDelete(IReadOnlyCollection<int> ids, bool left)
        {
            foreach (var id in ids)
            {
                var joinedRowId = left
                    ? RemoveMappingBySourceRowId(_leftRowIdToJoinedRowIdMapping, id)
                    : RemoveMappingBySourceRowId(_rightRowIdToJoinedRowIdMapping, id);

                if (joinedRowId != -1)
                {
                    RemoveId(joinedRowId);
                }
            }
        }

        private int CreateMapping(int leftId, int rightId)
        {
            if (_freeRows.Count > 0)
            {
                var oldRowIndex = _freeRows.First();
                _freeRows.Remove(oldRowIndex);
                return oldRowIndex;
            }
            else
            {
                _latestJoinedRowId++;

                _leftRowIdToJoinedRowIdMapping.Add(leftId, _latestJoinedRowId);
                _rightRowIdToJoinedRowIdMapping.Add(rightId, _latestJoinedRowId);
                return _latestJoinedRowId;
            }
        }

        private int RemoveMappingBySourceRowId(Dictionary<int, int> sourceRowIdToJoinedRowIdMapping, int sourceRowId)
        {
            if (sourceRowIdToJoinedRowIdMapping.TryGetValue(sourceRowId, out int joinedRowId))
            {
                _freeRows.Add(joinedRowId);
                _leftRowIdToJoinedRowIdMapping.Remove(sourceRowId);
                return joinedRowId;
            }

            return -1;
        }

        void _rightChangeTracker_OnSyncPointArrived()
        {
            FireChanges();
        }

        void _rightChangeTracker_OnSchemaArrived(ISchema schema)
        {
            _rightSchema = schema;
        }

        void _rightChangeTracker_OnUpdated(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            throw new System.NotImplementedException();
        }

        void _rightChangeTracker_OnDeleted(IReadOnlyCollection<int> ids)
        {
            ProcessOnDelete(ids, false);
        }

        void _rightChangeTracker_OnAdded(IReadOnlyCollection<int> ids)
        {
            ProcessOnAdd(ids, _allRightRowIds, (IReadOnlyCollection<int>)_allLeftRowIds, _rightSchema, _leftSchema);
        }
    }
}

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

        private readonly IReadOnlyCollection<int> _allLeftRowIds;
        private readonly IReadOnlyCollection<int> _allRightRowIds;

        private readonly Dictionary<int, List<int>> _leftRowIdToJoinedRowIdMapping;
        private readonly Dictionary<int, List<int>> _rightRowIdToJoinedRowIdMapping;

        private int _latestJoinedRowId = -1;

        private readonly HashSet<int> _freeRows;

        private HashSet<IField> _currentUpdatedFields;

        private Dictionary<int, RowMap> _joinedRowIdsToLeftRightRowIdsMapping;

        private JoinProjectedFieldsMeta _projectionFieldsMeta;

        private KeyFieldsMeta _keyFieldsMeta;

        public Join(IDataProvider leftSource,
                    IDataProvider rightSource,
                    JoinType joinType,
                    KeyFieldsMeta keyFieldsMeta,
                    JoinProjectedFieldsMeta projectionFieldsMeta)
        {
            _freeRows = new HashSet<int>();

            _joinedRowIdsToLeftRightRowIdsMapping = new Dictionary<int, RowMap>();

            _currentUpdatedFields = new HashSet<IField>();
            _allLeftRowIds = leftSource.RowIds;
            _allRightRowIds = rightSource.RowIds;

            _leftRowIdToJoinedRowIdMapping = new Dictionary<int, List<int>>();
            _rightRowIdToJoinedRowIdMapping = new Dictionary<int, List<int>>();

            _keyFieldsMeta = keyFieldsMeta;

            _projectionFieldsMeta = projectionFieldsMeta;

            _leftChangeTracker = new ChangeTracker();
            _leftChangeTracker.OnAdded += _leftChangeTracker_OnAdded;
            _leftChangeTracker.OnDeleted += _leftChangeTracker_OnDeleted;
            _leftChangeTracker.OnUpdated += _leftChangeTracker_OnUpdated;
            _leftChangeTracker.OnSchemaArrived += _leftChangeTracker_OnSchemaArrived;
            _leftChangeTracker.OnSyncPointArrived += _leftChangeTracker_OnSyncPointArrived;
            _leftChangeTracker.SetSource(leftSource);

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
            if (_rightSchema != null)
            {
                FireChanges();
            }
        }

        void _leftChangeTracker_OnSchemaArrived(ISchema schema)
        {
            _leftSchema = schema;

            var projectedSchema = new JoinProjectionSchema(_projectionFieldsMeta.ProjectedFields, _joinedRowIdsToLeftRightRowIdsMapping)
            {
                LeftSchema = schema
            };

            SetSchema(projectedSchema);
        }

        void _leftChangeTracker_OnUpdated(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            ProcessOnUpdated(ids, updatedFields, true);
        }

        private void ProcessOnUpdated(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields, bool left)
        {
            // Lets check if any fields which are used in the key are updated.
            // This will cause either add or delete or delete/add
            var keyChanged = false;
            var keyFieldNames = left ? _keyFieldsMeta.LeftKeyFieldNames : _keyFieldsMeta.RightKeyFieldNames;

            foreach (var updatedField in updatedFields)
            {
                if (keyFieldNames.Contains(updatedField.Name))
                {
                    keyChanged = true;
                    break;
                }
            }

            if (keyChanged)
            {
                ProcessOnDelete(ids, left);
                ProcessOnAdd(ids, left);
            }
            else // non-key updated
            {
                _currentUpdatedFields.Clear();

                var oldFieldNamesToNewFieldNamesMapping = left
                                                            ? _projectionFieldsMeta.LeftSourceOldNamesToNewNamesMapping
                                                            : _projectionFieldsMeta.RightSourceOldNamesToNewNamesMapping;

                foreach (var updatedField in updatedFields)
                {
                    if (oldFieldNamesToNewFieldNamesMapping.TryGetValue(updatedField.Name, out ISet<string> newFieldNames))
                    {
                        foreach (var newFieldName in newFieldNames)
                        {
                            _currentUpdatedFields.Add(Schema.GetFieldMeta(newFieldName));
                        }
                    }
                }

                if (_currentUpdatedFields.Count > 0)
                {
                    var somethingWasUpdated = false;

                    var rowIdToJoinedRowIdMapping = left ? _leftRowIdToJoinedRowIdMapping : _rightRowIdToJoinedRowIdMapping;

                    foreach (var id in ids)
                    {
                        if (rowIdToJoinedRowIdMapping.TryGetValue(id, out List<int> joinedRowIds))
                        {
                            foreach (var joinedRowId in joinedRowIds)
                            {
                                UpdateId(joinedRowId);
                                somethingWasUpdated = true;
                            }
                        }
                    }

                    if (somethingWasUpdated)
                    {
                        foreach (var updatedField in _currentUpdatedFields)
                        {
                            AddUpdatedField(updatedField);
                        }
                    }
                }
            }
        }

        void _leftChangeTracker_OnDeleted(IReadOnlyCollection<int> ids)
        {
            ProcessOnDelete(ids, true);
        }

        void _leftChangeTracker_OnAdded(IReadOnlyCollection<int> ids)
        {
            ProcessOnAdd(ids, true);
        }

        private void ProcessOnAdd(IReadOnlyCollection<int> ids, bool left)
        {
            var allLeftIds = left ? _allLeftRowIds : _allRightRowIds;

            var outerIds = left ? _allRightRowIds : _allLeftRowIds;

            foreach (var id in ids)
            {
                foreach (var outerRowId in outerIds)
                {
                    var leftRowId = left ? id : outerRowId;
                    var rightRowId = left ? outerRowId : id;

                    if (_keyFieldsMeta.RowMatcher(_leftSchema, leftRowId, _rightSchema, rightRowId))
                    {
                        var joinedRowId = CreateMapping(leftRowId, rightRowId);
                        AddId(joinedRowId);
                    }
                }
            }
        }

        private void ProcessOnDelete(IReadOnlyCollection<int> ids, bool left)
        {
            var leftRowIdToJoinedRowIdMapping = left ? _leftRowIdToJoinedRowIdMapping : _rightRowIdToJoinedRowIdMapping;
            var rightRowIdToJoinedRowIdMapping = left ? _rightRowIdToJoinedRowIdMapping : _leftRowIdToJoinedRowIdMapping;
            var allLeftIds = left ? _allLeftRowIds : _allRightRowIds;

            foreach (var id in ids)
            {
                if (leftRowIdToJoinedRowIdMapping.TryGetValue(id, out List<int> joinedRowIds))
                {
                    foreach (var joinedRowId in joinedRowIds)
                    {
                        _freeRows.Add(joinedRowId);

                        var leftRightMap = _joinedRowIdsToLeftRightRowIdsMapping[joinedRowId];
                        _joinedRowIdsToLeftRightRowIdsMapping.Remove(joinedRowId);

                        var counterRowId = left ? leftRightMap.RightRowId : leftRightMap.LeftRowId;

                        var counterJoinedRowIds = rightRowIdToJoinedRowIdMapping[counterRowId];
                        counterJoinedRowIds.Remove(joinedRowId);

                        RemoveId(joinedRowId);
                    }
                    joinedRowIds.Clear();
                }
            }
        }

        private int CreateMapping(int leftId, int rightId)
        {
            int newJoinedRowId;
            if (_freeRows.Count > 0)
            {
                var oldRowIndex = _freeRows.First();
                _freeRows.Remove(oldRowIndex);
                newJoinedRowId = oldRowIndex;
            }
            else
            {
                _latestJoinedRowId++;

                newJoinedRowId = _latestJoinedRowId;
            }

            // add mapping 

            _joinedRowIdsToLeftRightRowIdsMapping.Add(newJoinedRowId, new RowMap(leftId, rightId));

            if (!_leftRowIdToJoinedRowIdMapping.TryGetValue(leftId, out List<int> joinedRowIds))
            {
                joinedRowIds = new List<int>();
                _leftRowIdToJoinedRowIdMapping.Add(leftId, joinedRowIds);
            }

            joinedRowIds.Add(newJoinedRowId);


            if (!_rightRowIdToJoinedRowIdMapping.TryGetValue(rightId, out joinedRowIds))
            {
                joinedRowIds = new List<int>();
                _rightRowIdToJoinedRowIdMapping.Add(rightId, joinedRowIds);
            }

            joinedRowIds.Add(newJoinedRowId);

            return newJoinedRowId;
        }

        void _rightChangeTracker_OnSyncPointArrived()
        {
            FireChanges();
        }

        void _rightChangeTracker_OnSchemaArrived(ISchema schema)
        {
            _rightSchema = schema;
            ((JoinProjectionSchema)Schema).RightSchema = schema;
        }

        void _rightChangeTracker_OnUpdated(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            ProcessOnUpdated(ids, updatedFields, false);
        }

        void _rightChangeTracker_OnDeleted(IReadOnlyCollection<int> ids)
        {
            ProcessOnDelete(ids, false);
        }

        void _rightChangeTracker_OnAdded(IReadOnlyCollection<int> ids)
        {
            ProcessOnAdd(ids, false);
        }
    }
}

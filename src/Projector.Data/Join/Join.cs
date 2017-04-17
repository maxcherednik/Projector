using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    public class Join : DataProviderBase, IDataProvider
    {
        private ChangeTracker _leftChangeTracker;
        private ChangeTracker _rightChangeTracker;

        private ISchema _leftSchema;
        private ISchema _rightSchema;

        private Func<ISchema, int, string> _leftKeySelector;
        private Func<ISchema, int, string> _rightKeySelector;

        private Dictionary<string, List<int>> _leftValues;
        private Dictionary<string, List<int>> _rightValues;

        public Join(IDataProvider leftSource,
                    IDataProvider rightSource,
                    JoinType joinType,
                    IDictionary<string, IField> projectionFields)
        {
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
            throw new System.NotImplementedException();
        }

        void _leftChangeTracker_OnSchemaArrived(ISchema schema)
        {
            _leftSchema = schema;
        }

        void _leftChangeTracker_OnUpdated(IReadOnlyCollection<int> arg1, IReadOnlyCollection<IField> arg2)
        {
            throw new System.NotImplementedException();
        }

        void _leftChangeTracker_OnDeleted(IReadOnlyCollection<int> obj)
        {
            throw new System.NotImplementedException();
        }

        void _leftChangeTracker_OnAdded(IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids)
            {
                var leftKey = _leftKeySelector(_leftSchema, id);

                List<int> leftIds;
                if (!_leftValues.TryGetValue(leftKey, out leftIds))
                {
                    leftIds = new List<int>();
                    _leftValues.Add(leftKey, leftIds);

                }

                leftIds.Add(id);

                List<int> rightIds;
                if (_rightValues.TryGetValue(leftKey, out rightIds))
                {
                    foreach (var rightId in rightIds)
                    {
                        PushMapping(id, rightId);
                    }
                }
            }
        }

        private void PushMapping(int leftId, int rightId)
        {

        }

        private void RemoveMapping(int leftId, int rightId)
        {

        }

        void _rightChangeTracker_OnSyncPointArrived()
        {
            throw new System.NotImplementedException();
        }

        void _rightChangeTracker_OnSchemaArrived(ISchema schema)
        {
            _rightSchema = schema;
        }

        void _rightChangeTracker_OnUpdated(IReadOnlyCollection<int> arg1, IReadOnlyCollection<IField> arg2)
        {
            throw new System.NotImplementedException();
        }

        void _rightChangeTracker_OnDeleted(IReadOnlyCollection<int> obj)
        {
            throw new System.NotImplementedException();
        }

        void _rightChangeTracker_OnAdded(IReadOnlyCollection<int> ids)
        {
            foreach (var id in ids)
            {
                var rightKey = _rightKeySelector(_rightSchema, id);

                List<int> rightIds;
                if (!_rightValues.TryGetValue(rightKey, out rightIds))
                {
                    rightIds = new List<int>();
                    _rightValues.Add(rightKey, rightIds);

                }

                rightIds.Add(id);

                List<int> leftIds;
                if (_leftValues.TryGetValue(rightKey, out leftIds))
                {
                    foreach (var leftId in leftIds)
                    {
                        PushMapping(leftId, id);
                    }
                }
            }
        }




    }
}

using System;
using System.Linq;
using System.Collections.Generic;

namespace Projector.Data
{
    public class DataProviderBase : IDataProvider
    {
        private ISchema _schema;

        protected HashSet<int> UsedIds;

        private HashSet<int> _currentAddedIds;
        private HashSet<int> _currentRemovedIds;

        public DataProviderBase ()
        {
            UsedIds = new HashSet<int> ();
            _currentAddedIds = new HashSet<int> ();
            _currentRemovedIds = new HashSet<int> ();
        }

        protected void SetSchema (ISchema schema)
        {
            _schema = schema;
        }

        protected void AddId (int id)
        {
            if (!_currentAddedIds.Add (id)) {
                throw new InvalidOperationException ("Duplicate key in the add collection");
            }
        }

        protected void RemoveId (int id)
        {
            if (!_currentRemovedIds.Add (id)) {
                throw new InvalidOperationException ("Duplicate key in the remove collection");
            }
        }

        protected void FireChanges ()
        {
            var thereWereChanges = false;
            foreach (var newId in _currentAddedIds) {
                UsedIds.Add (newId);
            }

            foreach (var removeId in _currentRemovedIds) {
                UsedIds.Remove (removeId);
            }

            if (_currentRemovedIds.Count > 0) {
                thereWereChanges = true;
                FireOnDelete (_currentRemovedIds.ToList ());
                _currentRemovedIds.Clear ();
            }

            if (_currentAddedIds.Count > 0) {
                thereWereChanges = true;
                FireOnAdd (_currentAddedIds.ToList ());
                _currentAddedIds.Clear ();
            }

            if (thereWereChanges) {
                FireOnSyncPoint ();
            }
        }

        public IDisconnectable AddConsumer (IDataConsumer consumer)
        {
            consumer.OnSchema (_schema);

            if (UsedIds.Count > 0) {
                consumer.OnAdd (UsedIds.ToList ());
            }

            consumer.OnSyncPoint ();

            OnAdd += consumer.OnAdd;
            OnDelete += consumer.OnDelete;
            OnUpdate += consumer.OnUpdate;
            OnSchema += consumer.OnSchema;
            OnSyncPoint += consumer.OnSyncPoint;

            return new Disconnectable (this, consumer);
        }

        public void RemoveConsumer (IDataConsumer consumer)
        {
            OnAdd -= consumer.OnAdd;
            OnDelete -= consumer.OnDelete;
            OnUpdate -= consumer.OnUpdate;
            OnSchema -= consumer.OnSchema;
            OnSyncPoint -= consumer.OnSyncPoint;
        }

        public ISchema Schema { get { return _schema; } }

        private event Action<IList<int>> OnAdd;
        private event Action<IList<int>, IList<IField>> OnUpdate;
        private event Action<IList<int>> OnDelete;
        private event Action<ISchema> OnSchema;
        private event Action OnSyncPoint;

        private void FireOnAdd (IList<int> ids)
        {
            var handler = OnAdd;
            if (handler != null) {
                handler (ids);
            }
        }

        private void FireOnDelete (IList<int> ids)
        {
            var handler = OnDelete;
            if (handler != null) {
                handler (ids);
            }
        }

        private void FireOnUpdate (IList<int> ids, IList<IField> fields)
        {
            var handler = OnUpdate;
            if (handler != null) {
                handler (ids, fields);
            }
        }

        private void FireOnSchema (ISchema schema)
        {
            var handler = OnSchema;
            if (handler != null) {
                handler (schema);
            }
        }

        private void FireOnSyncPoint ()
        {
            var handler = OnSyncPoint;
            if (handler != null) {
                handler ();
            }
        }
    }
}

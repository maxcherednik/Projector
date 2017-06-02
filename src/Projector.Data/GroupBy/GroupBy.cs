using System;
using System.Collections.Generic;

namespace Projector.Data.GroupBy
{
    public class GroupBy : DataProviderBase, IDataConsumer
    {
        private IDisconnectable _subscription;

        public GroupBy(IDataProvider sourceDataProvider)
        {
            _subscription = sourceDataProvider.AddConsumer(this);
        }

        public void OnAdd(IReadOnlyCollection<int> ids)
        {
            throw new NotImplementedException();
        }

        public void OnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            throw new NotImplementedException();
        }

        public void OnDelete(IReadOnlyCollection<int> ids)
        {
            throw new NotImplementedException();
        }

        public void OnSchema(ISchema schema)
        {
            throw new NotImplementedException();
        }

        public void OnSyncPoint()
        {
            throw new NotImplementedException();
        }
    }
}

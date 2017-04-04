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

        public void OnAdd(IList<int> ids)
        {
            throw new NotImplementedException();
        }

        public void OnUpdate(IList<int> ids, IList<IField> updatedFields)
        {
            throw new NotImplementedException();
        }

        public void OnDelete(IList<int> ids)
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

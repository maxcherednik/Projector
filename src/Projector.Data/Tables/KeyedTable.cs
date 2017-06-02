using System;
using System.Collections.Generic;

namespace Projector.Data.Tables
{
    public class KeyedTable : IDataProvider
    {
        public IReadOnlyCollection<int> RowIds => throw new NotImplementedException();

        public KeyedTable(int capacity)
        {

        }

        public void AddConsumer(IDataConsumer consumer)
        {

        }

        IDisconnectable IDataProvider.AddConsumer(IDataConsumer consumer)
        {
            throw new NotImplementedException();
        }

        public void RemoveConsumer(IDataConsumer consumer)
        {
            throw new NotImplementedException();
        }
    }
}

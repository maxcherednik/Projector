using System;
using System.Collections.Generic;

namespace Projector.Data.Test.Helpers
{
    class TestConsumer : IDataConsumer
    {
        private ISchema _schema;

        private int _syncPointCalled;
        private int _schemaArrivedCounter;

        public int RowCount { get; private set; }

        public int UpdatedRowCount { get; private set; }

        public int CallsReceived { get; private set; }

        public void OnAdd(IReadOnlyCollection<int> rowIds)
        {
            CallsReceived++;
            foreach (var rowId in rowIds)
            {
                RowCount++;
            }
        }

        public void OnDelete(IReadOnlyCollection<int> rowIds)
        {
            CallsReceived++;
            foreach (var rowId in rowIds)
            {
                RowCount--;
            }
        }

        public void OnSchema(ISchema schema)
        {
            CallsReceived++;
            _schema = schema;
            _schemaArrivedCounter++;
        }

        public void OnSyncPoint()
        {
            CallsReceived++;
            _syncPointCalled++;
        }

        public void OnUpdate(IReadOnlyCollection<int> rowIds, IReadOnlyCollection<IField> updatedFields)
        {
            CallsReceived++;
            foreach (var rowId in rowIds)
            {
                UpdatedRowCount++;
            }
        }

        public void CleanCallsCounter()
        {
            CallsReceived = 0;
        }
    }
}

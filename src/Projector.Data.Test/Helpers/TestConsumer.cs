using System.Collections.Generic;

namespace Projector.Data.Test.Helpers
{
    internal class TestConsumer : IDataConsumer
    {
        public int RowCount { get; private set; }

        public int UpdatedRowCount { get; private set; }

        public int CallsReceived { get; private set; }

        public void OnAdd(IReadOnlyCollection<int> rowIds)
        {
            CallsReceived++;
            foreach (var _ in rowIds)
            {
                RowCount++;
            }
        }

        public void OnDelete(IReadOnlyCollection<int> rowIds)
        {
            CallsReceived++;
            foreach (var _ in rowIds)
            {
                RowCount--;
            }
        }

        public void OnSchema(ISchema schema)
        {
            CallsReceived++;
        }

        public void OnSyncPoint()
        {
            CallsReceived++;
        }

        public void OnUpdate(IReadOnlyCollection<int> rowIds, IReadOnlyCollection<IField> updatedFields)
        {
            CallsReceived++;
            foreach (var _ in rowIds)
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

using System;
using System.Collections.Generic;
using Projector.Data;

namespace Projector.Playground
{
    internal class ConsoleConsumer : IDataConsumer
    {
        private int _rowCounter;
        private ISchema _schema;

        public void OnAdd (IReadOnlyCollection<int> ids)
        {
            _rowCounter += ids.Count;
            Console.WriteLine (DateTime.Now + " OnAdd arrived. Row count: " + ids.Count + " Total count: " + _rowCounter);
            foreach (var id in ids) {
                Console.WriteLine ("Row id" + id);
            }
        }

        public void OnUpdate (IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            Console.WriteLine ("OnUpdate arrived");
        }

        public void OnDelete (IReadOnlyCollection<int> ids)
        {
            Console.WriteLine ("OnDelete arrived");
            foreach (var id in ids) {
                Console.WriteLine ("Row id" + id);
            }
        }

        public void OnSchema (ISchema schema)
        {
            _schema = schema;
            Console.WriteLine ("Schema arrived");

            foreach (var column in schema.Columns) {
                Console.WriteLine ("Field: " + column.Name + " of type: " + column.DataType);
            }
        }

        public void OnSyncPoint ()
        {
            Console.WriteLine ("SyncPoint arrived");
        }
    }
}

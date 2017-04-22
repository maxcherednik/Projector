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
            //foreach (var id in ids) {
            //    Console.WriteLine ("Row id" + id);
            //}
        }

        public void OnUpdate (IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            Console.WriteLine(DateTime.Now + " OnUpdate arrived. Row count: " + ids.Count);
            foreach (var id in ids)
            {
                var rowStr = "Row id: " + id;
                foreach (var updatedField in updatedFields)
                {
                    
                    if(updatedField.DataType == typeof(long))
                    {
                        rowStr += ", " + updatedField.Name + ": " + _schema.GetField<long>(updatedField.Name).GetValue(id);
                    }
                    else if(updatedField.DataType == typeof(string))
                    {
                        rowStr += ", " + updatedField.Name + ": " + _schema.GetField<string>(updatedField.Name).GetValue(id);
                    }
                    else if (updatedField.DataType == typeof(int))
                    {
                        rowStr += ", " + updatedField.Name + ": " + _schema.GetField<int>(updatedField.Name).GetValue(id);
                    }
                }

                Console.WriteLine(rowStr);
            }
        }

        public void OnDelete (IReadOnlyCollection<int> ids)
        {
            _rowCounter -= ids.Count;
            Console.WriteLine ("OnDelete arrived " + ids.Count);
            //foreach (var id in ids) {
            //    Console.WriteLine ("Row id" + id);
            //}
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

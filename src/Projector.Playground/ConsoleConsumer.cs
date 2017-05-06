using System;
using System.Collections.Generic;
using Projector.Data;
using System.Text;

namespace Projector.Playground
{
    internal class ConsoleConsumer : IDataConsumer
    {
        private int _rowCounter;
        private ISchema _schema;
        private bool _printDetailes;
        private StringBuilder _stringBuilder;

        public ConsoleConsumer(bool printDetailes)
        {
            _printDetailes = printDetailes;
            _stringBuilder = new StringBuilder();
        }

        public void OnAdd(IReadOnlyCollection<int> ids)
        {
            _rowCounter += ids.Count;
            Console.WriteLine(DateTime.Now + " OnAdd arrived. Row count: " + ids.Count + " Total count: " + _rowCounter);
            //foreach (var id in ids) {
            //    Console.WriteLine ("Row id" + id);
            //}
        }

        public void OnUpdate(IReadOnlyCollection<int> ids, IReadOnlyCollection<IField> updatedFields)
        {
            Console.WriteLine(DateTime.Now + " OnUpdate arrived. Row count: " + ids.Count);
            if (_printDetailes)
            {
                

                foreach (var id in ids)
                {
                    _stringBuilder.Append("Row id: ").Append(id);
                    foreach (var updatedField in updatedFields)
                    {

                        _stringBuilder.Append(", ");
                        _stringBuilder.Append(updatedField.Name);
                        _stringBuilder.Append(": ");

                        if (updatedField.DataType == typeof(long))
                        {
                            _stringBuilder.Append(_schema.GetField<long>(updatedField.Name).GetValue(id));
                        }
                        else if (updatedField.DataType == typeof(string))
                        {
                            _stringBuilder.Append(_schema.GetField<string>(updatedField.Name).GetValue(id));
                        }
                        else if (updatedField.DataType == typeof(int))
                        {
                            _stringBuilder.Append(_schema.GetField<int>(updatedField.Name).GetValue(id));
                        }
                    }
                    _stringBuilder.AppendLine();
                }

                Console.WriteLine(_stringBuilder);

                _stringBuilder.Clear();
            }
        }

        public void OnDelete(IReadOnlyCollection<int> ids)
        {
            _rowCounter -= ids.Count;
            Console.WriteLine("OnDelete arrived " + ids.Count);
            //foreach (var id in ids) {
            //    Console.WriteLine ("Row id" + id);
            //}
        }

        public void OnSchema(ISchema schema)
        {
            _schema = schema;
            Console.WriteLine("Schema arrived");

            foreach (var column in schema.Columns)
            {
                Console.WriteLine("Field: " + column.Name + " of type: " + column.DataType);
            }
        }

        public void OnSyncPoint()
        {
            Console.WriteLine("SyncPoint arrived");
        }
    }
}

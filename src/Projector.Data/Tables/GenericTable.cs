using System.Reflection;
using System;


namespace Projector.Data.Tables
{
    public class Table<T> : Table, IDataProvider<T>
    {
        public Table (int capacity)
            : base (CreateSchema<T> (capacity))
        {

        }

        public Table ()
            : this (1024)
        {
        }

        private static Schema CreateSchema<Tsource> (int capacity)
        {
            var t = typeof (Tsource);

            var propInfos = t.GetTypeInfo ().DeclaredProperties;

            var schema = new Schema (capacity);

            foreach (var propInfoItem in propInfos) {
                if (propInfoItem.PropertyType == typeof (string)) {
                    schema.CreateField<string> (propInfoItem.Name);
                } else if (propInfoItem.PropertyType == typeof (int)) {
                    schema.CreateField<int> (propInfoItem.Name);
                } else if (propInfoItem.PropertyType == typeof (long)) {
                    schema.CreateField<long> (propInfoItem.Name);
                } else {
                    throw new InvalidOperationException ("Type: " + propInfoItem.PropertyType + " is not supported");
                }
            }

            return schema;
        }
    }
}

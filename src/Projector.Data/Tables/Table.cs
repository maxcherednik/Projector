using System.Collections.Generic;

namespace Projector.Data.Tables
{
    public class Table : DataProviderBase
    {
        private readonly IWritebleSchema _schema;

        private readonly List<int> _idsUpdated;

        public Table(IWritebleSchema schema)
        {
            _idsUpdated = new List<int>();
            _schema = schema;
            SetSchema(_schema);
        }

        public void Set<T>(int rowIndex, string name, T value)
        {
            var writableField = _schema.GetWritableField<T>(name);
            writableField.SetValue(rowIndex, value);
            if (!CurrentAddedIds.Contains(rowIndex))
            {
                UpdateId(rowIndex, writableField);
            }
        }

        public int NewRow()
        {
            var newRowId = _schema.GetNewRowId();
            AddId(newRowId);
            return newRowId;
        }

        public void RemoveRow(int rowIndex)
        {
            _schema.Remove(rowIndex);
            RemoveId(rowIndex);
        }

        public new void FireChanges()
        {
            base.FireChanges();
        }
    }
}

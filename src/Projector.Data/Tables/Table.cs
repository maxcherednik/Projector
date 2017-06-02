using System.Collections.Generic;

namespace Projector.Data.Tables
{
    public class Table : DataProviderBase
    {
        private readonly IWritebleSchema _schema;

        private readonly HashSet<int> _usedRowIds;

        public Table(IWritebleSchema schema)
        {
            _usedRowIds = new HashSet<int>();
            _schema = schema;
            SetSchema(_schema);
            SetRowIds((IReadOnlyCollection<int>)_usedRowIds);
        }

        public void Set<T>(int rowIndex, string name, T value)
        {
            var writableField = _schema.GetWritableField<T>(name);
            writableField.SetValue(rowIndex, value);

            if (!CurrentAddedIds.Contains(rowIndex))
            {
                UpdateId(rowIndex);
                AddUpdatedField(writableField);
            }
        }

        public int NewRow()
        {
            var newRowId = _schema.GetNewRowId();
            _usedRowIds.Add(newRowId);
            AddId(newRowId);
            return newRowId;
        }

        public void RemoveRow(int rowIndex)
        {
            _schema.Remove(rowIndex);
            _usedRowIds.Remove(rowIndex);
            RemoveId(rowIndex);
        }

        public new void FireChanges()
        {
            base.FireChanges();
        }
    }
}

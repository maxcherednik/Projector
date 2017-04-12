using System;
using System.Collections.Generic;
using System.Linq;

namespace Projector.Data.Tables
{
    public class Schema : IWritebleSchema
    {
        private readonly Dictionary<string, IWritableField> _data;
        private readonly List<IWritableField> _columnList;
        private readonly int _capacity;

        private readonly HashSet<int> _freeRows;

        private int _currentRowIndex = -1;

        public Schema(int capacity)
        {
            _capacity = capacity;
            _data = new Dictionary<string, IWritableField>();
            _columnList = new List<IWritableField>();
            _freeRows = new HashSet<int>();
        }

        public IReadOnlyList<IField> Columns
        {
            get { return _columnList; }
        }

        public IField<T> GetField<T>(int id, string name)
        {
            IWritableField field;
            if (_data.TryGetValue(name, out field))
            {
                field.SetCurrentRow(id);
                return (IField<T>)field;
            }

            throw new InvalidOperationException("Can't find column name: '" + name + "'");
        }

        public IWritableField<T> GetWritableField<T>(int id, string name)
        {
            IWritableField field;
            if (_data.TryGetValue(name, out field))
            {
                field.SetCurrentRow(id);
                return (IWritableField<T>)field;
            }

            throw new InvalidOperationException("Can't find column name: '" + name + "'");
        }

        public void CreateField<T>(string name)
        {
            var field = new Field<T>(new List<T>(_capacity), name);
            _data.Add(name, field);
            _columnList.Add(field);
        }

        public int GetNewRowId()
        {
            if (_freeRows.Count > 0)
            {
                var oldRowIndex = _freeRows.First();
                _freeRows.Remove(oldRowIndex);
                foreach (var writableField in _columnList)
                {
                    writableField.CleanOldValue(oldRowIndex);
                }
                return oldRowIndex;
            }
            else
            {
                _currentRowIndex++;
                foreach (var writableField in _columnList)
                {
                    writableField.EnsureCapacity(_currentRowIndex);
                }

                return _currentRowIndex;
            }
        }

        public void Remove(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex > _currentRowIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), rowIndex, "Index was out of range. Must be non-negative and less than the size of the collection");
            }

            if (!_freeRows.Add(rowIndex))
            {
                throw new ArgumentException("Row id " + rowIndex + " already removed");
            }
        }
    }
}

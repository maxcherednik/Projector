using System;
using System.Collections.Generic;

namespace Projector.Data.Tables
{
    public class Field<TData> : IField<TData>, IWritableField<TData>
    {
        private int _id;

        private List<TData> _data;

        public Field(List<TData> data, string name)
        {
            _data = data;
            Name = name;
        }

        public Type DataType => typeof(TData);

        public TData Value => _data[_id];

        public string Name { get; }

        void IWritableField<TData>.SetValue(TData value)
        {
            _data[_id] = value;
        }

        public void SetCurrentRow(int rowId)
        {
            _id = rowId;
        }

        public void EnsureCapacity(int rowId)
        {
            if (rowId >= _data.Count)
            {
                _data.Add(default(TData));
            }
        }

        public void CleanOldValue(int rowId)
        {
            _data[rowId] = default(TData);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Projector.Data.Tables
{
    public class Field<TData> : IWritableField<TData>
    {
        private readonly List<TData> _data;

        public Field(List<TData> data, string name)
        {
            _data = data;
            Name = name;
        }

        public Type DataType => typeof(TData);

        public TData GetValue(int rowId)
        {
            return _data[rowId];
        }

        public string Name { get; }

        void IWritableField<TData>.SetValue(int rowId, TData value)
        {
            _data[rowId] = value;
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

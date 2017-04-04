using System;
using System.Collections.Generic;

namespace Projector.Data.Tables
{
    public class Field<TData> : IField<TData>, IWritableField<TData>
    {
        private int _id;
        private List<TData> _data;
        private readonly string _name;

        public Field(List<TData> data, string name)
        {
            _data = data;
            _name = name;
        }

        public Type DataType
        {
            get { return typeof(TData); }
        }

        public TData Value
        {
            get
            {
                return _data[_id];
            }
        }

        void IWritableField<TData>.SetValue(TData value)
        {
            _data[_id] = value;
        }


        public string Name
        {
            get { return _name; }
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
    }
}

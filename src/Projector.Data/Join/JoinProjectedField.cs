﻿using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    public class JoinProjectedField<TData> : IField<TData>
    {
        private readonly Func<ISchema, int, ISchema, int, TData> _fieldAccessor;

        private ISchema _leftSchema;
        private ISchema _rightSchema;

        private IDictionary<int, Tuple<int, int>> _rowIdMap;

        public JoinProjectedField(string name, Func<ISchema, int, ISchema, int, TData> fieldAccessor)
        {
            Name = name;
            _fieldAccessor = fieldAccessor;
        }

        public void SetLeftSchema(ISchema schema)
        {
            _leftSchema = schema;
        }

        public void SetRightSchema(ISchema schema)
        {
            _rightSchema = schema;
        }

        public void SetRowIdMap(IDictionary<int, Tuple<int, int>> rowIdMap)
        {
            _rowIdMap = rowIdMap;
        }

        public TData GetValue(int rowId)
        {
            var oldRowIds = _rowIdMap[rowId];
            return _fieldAccessor(_leftSchema, oldRowIds.Item1, _rightSchema, oldRowIds.Item2);
        }

        public Type DataType => typeof(TData);

        public string Name { get; }
    }
}

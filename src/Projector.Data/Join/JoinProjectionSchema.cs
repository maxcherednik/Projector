using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    class JoinProjectionSchema : ISchema
    {
        private readonly IDictionary<string, IField> _data;
        private IDictionary<int, Tuple<int, int>> _joinedRowIdsToLeftRightRowIdsMapping;

        public JoinProjectionSchema(IDictionary<string, IField> projectionFields, IDictionary<int, Tuple<int, int>> joinedRowIdsToLeftRightRowIdsMapping)
        {
            _data = projectionFields;
            _joinedRowIdsToLeftRightRowIdsMapping = joinedRowIdsToLeftRightRowIdsMapping;
            Columns = new List<IField>(_data.Values);
        }

        public IReadOnlyList<IField> Columns { get; }

        public IField<T> GetField<T>(string name)
        {
            if (_data.TryGetValue(name, out IField projectionField))
            {
                var projectedFieldImpl = (JoinProjectedField<T>)projectionField;
                projectedFieldImpl.SetLeftSchema(LeftSchema);
                projectedFieldImpl.SetRightSchema(RightSchema);
                projectedFieldImpl.SetRowIdMap(_joinedRowIdsToLeftRightRowIdsMapping);
                return projectedFieldImpl;
            }

            throw new InvalidOperationException("Can't find column name: '" + name + "'");
        }

        public IField GetFieldMeta(string name)
        {
            if (_data.TryGetValue(name, out IField field))
            {
                return field;
            }

            throw new InvalidOperationException("Can't find column name: '" + name + "'");
        }

        public ISchema LeftSchema { get; set; }

        public ISchema RightSchema { get; set; }
    }
}

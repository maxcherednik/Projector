using System;

namespace Projector.Data.Join
{
    public class JoinProjectedField<TData> : IField<TData>
    {
        private readonly Func<ISchema, int, ISchema, int, TData> _fieldAccessor;

        private ISchema _leftSchema;
        private ISchema _rightSchema;

        private int _leftId;
        private int _rightId;

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

        public void SetLeftCurrentRow(int id)
        {
            _leftId = id;
        }

        public void SetRightCurrentRow(int id)
        {
            _rightId = id;
        }

        public TData GetValue(int rowId)
        {
            return _fieldAccessor(_leftSchema, _leftId, _rightSchema, _rightId);
        }

        public Type DataType => typeof(TData);

        public string Name { get; }
    }
}

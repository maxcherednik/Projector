using System;

namespace Projector.Data.Projection
{
    public class ProjectedField<TData> : IField<TData>
    {
        private ISchema _schema;
        private readonly Func<ISchema, int, TData> _fieldAccessor;

        public ProjectedField(string name, Func<ISchema, int, TData> fieldAccessor)
        {
            Name = name;
            _fieldAccessor = fieldAccessor;
        }

        public void SetSchema(ISchema schema)
        {
            _schema = schema;
        }

        public TData GetValue(int rowId)
        {
            return _fieldAccessor(_schema, rowId);
        }

        public Type DataType => typeof(TData);

        public string Name { get; }
    }
}

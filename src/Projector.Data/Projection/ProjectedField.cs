using System;

namespace Projector.Data.Projection
{
    public class ProjectedField<TData> : IField<TData>
    {
        private ISchema _schema;
        private readonly string _name;
        private readonly Func<ISchema, int, TData> _fieldAccessor;

        public ProjectedField(string name, Func<ISchema, int, TData> fieldAccessor)
        {
            _name = name;
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

        public Type DataType
        {
            get { return typeof(TData); }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}

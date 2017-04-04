using System;

namespace Projector.Data.Projection
{
    public class ProjectedField<TData> : IField<TData>
    {
        private ISchema _schema;
        private readonly string _name;
        private readonly Func<ISchema, int, TData> _fieldAccessor;
        private int _id;

        public ProjectedField(string name, Func<ISchema, int, TData> fieldAccessor)
        {
            _name = name;
            _fieldAccessor = fieldAccessor;
        }

        public void SetSchema(ISchema schema)
        {
            _schema = schema;
        }

        public void SetCurrentRow(int id)
        {
            _id = id;
        }

        public TData Value
        {
            get { return _fieldAccessor(_schema, _id); }
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

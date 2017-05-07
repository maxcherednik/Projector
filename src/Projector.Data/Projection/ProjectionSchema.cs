using System;
using System.Collections.Generic;

namespace Projector.Data.Projection
{
    class ProjectionSchema : ISchema
    {
        private readonly ISchema _sourceSchema;
        private readonly IDictionary<string, IField> _data;
        private readonly List<IField> _columnList;

        public ProjectionSchema(ISchema schema, IDictionary<string, IField> projectionFields)
        {
            _sourceSchema = schema;
            _data = projectionFields;
            _columnList = new List<IField>(_data.Values);
        }

        public IReadOnlyList<IField> Columns
        {
            get { return _columnList; }
        }

        public IField<T> GetField<T>(string name)
        {
            if (_data.TryGetValue(name, out IField projectionField))
            {
                var projectedFieldImpl = (ProjectedField<T>)projectionField;
                projectedFieldImpl.SetSchema(_sourceSchema);
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
    }
}

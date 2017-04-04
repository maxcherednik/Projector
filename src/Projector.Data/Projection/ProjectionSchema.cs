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

        public IField<T> GetField<T>(int id, string name)
        {
            IField projectionField;
            if (_data.TryGetValue(name, out projectionField))
            {
                var projectedFieldImpl = (ProjectedField<T>)projectionField;
                projectedFieldImpl.SetSchema(_sourceSchema);
                projectedFieldImpl.SetCurrentRow(id);
                return projectedFieldImpl;

            }

            throw new InvalidOperationException("Can't find column name: '" + name + "'");
        }
    }
}

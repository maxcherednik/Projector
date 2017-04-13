using System;
using System.Collections.Generic;

namespace Projector.Data.Join
{
    class JoinProjectionSchema : ISchema
    {
        private readonly ISchema _sourceSchema;
        private readonly IDictionary<string, IField> _data;

        public JoinProjectionSchema(ISchema schema, IDictionary<string, IField> projectionFields)
        {
            _sourceSchema = schema;
            _data = projectionFields;
            Columns = new List<IField>(_data.Values);
        }

        public IReadOnlyList<IField> Columns { get; }

        public IField<T> GetField<T>(string name)
        {
            IField projectionField;
            if (_data.TryGetValue(name, out projectionField))
            {
                var projectedFieldImpl = (JoinProjectedField<T>)projectionField;
                projectedFieldImpl.SetLeftSchema(_sourceSchema);
                projectedFieldImpl.SetRightSchema(_sourceSchema);
                return projectedFieldImpl;
            }

            throw new InvalidOperationException("Can't find column name: '" + name + "'");
        }
    }
}

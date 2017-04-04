using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projector.Data.Join
{
    class JoinProjectionSchema: ISchema
    {
        private readonly ISchema _sourceSchema;
        private readonly IDictionary<string, IField> _data;
        private readonly List<IField> _columnList;

        public JoinProjectionSchema(ISchema schema, IDictionary<string, IField> projectionFields)
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
                var projectedFieldImpl = (JoinProjectedField<T>)projectionField;
                projectedFieldImpl.SetLeftSchema(_sourceSchema);
                projectedFieldImpl.SetRightSchema(_sourceSchema);
                projectedFieldImpl.SetLeftCurrentRow(id);
                projectedFieldImpl.SetRightCurrentRow(id);
                return projectedFieldImpl;

            }

            throw new InvalidOperationException("Can't find column name: '" + name + "'");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projector.Data.Join
{
    public class JoinProjectedField<TData> : IField<TData>
    {
        private readonly string _name;
        private readonly Func<ISchema, int,ISchema, int, TData> _fieldAccessor;

        private ISchema _leftSchema;
        private ISchema _rightSchema;
        
        private int _leftId;
        private int _rightId;

        public JoinProjectedField(string name, Func<ISchema, int, ISchema, int, TData> fieldAccessor)
        {
            _name = name;
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

        public TData Value
        {
            get { return _fieldAccessor(_leftSchema,_leftId,_rightSchema, _rightId); }
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

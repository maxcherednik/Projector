using System.Reflection;
using System;
using System.Linq.Expressions;

namespace Projector.Data.Tables
{
    public class Table<TSource> : Table, IDataProvider<TSource>
    {
        public Table(int capacity) : base(CreateSchema(capacity))
        {

        }

        public Table() : this(1024)
        {

        }

        private static Schema CreateSchema(int capacity)
        {
            var t = typeof(TSource);

            var propInfos = t.GetTypeInfo().DeclaredProperties;

            var schema = new Schema(capacity);

            var method = typeof(Schema).GetRuntimeMethod("CreateField", new[] { typeof(string) });

            foreach (var propInfoItem in propInfos)
            {
                var generic = method.MakeGenericMethod(propInfoItem.PropertyType);
                generic.Invoke(schema, new object[] { propInfoItem.Name });
            }

            return schema;
        }

        public void Set<TMember>(int rowIndex, Expression<Func<TSource, TMember>> field, TMember value)
        {
            var memberAccess = (MemberExpression)field.Body;
            Set(rowIndex, memberAccess.Member.Name, value);
        }
    }
}

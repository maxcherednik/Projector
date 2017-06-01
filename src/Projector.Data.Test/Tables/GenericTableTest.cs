using System.Linq;
using Projector.Data.Tables;
using Xunit;

namespace Projector.Data.Test.Tables
{
    public class GenericTableTest
    {
        [Fact]
        public void CreateTableTest()
        {
            var table = new Table<Person>();

            Assert.Equal(2, table.Schema.Columns.Count);

            var field1 = table.Schema.Columns.Single(x => x.Name == "Name");
            Assert.Equal(typeof(string), field1.DataType);

            var field2 = table.Schema.Columns.Single(x => x.Name == "Age");
            Assert.Equal(typeof(int), field2.DataType);
        }

        [Fact]
        public void SetFieldTest()
        {
            var table = new Table<Person>();

            var rowId = table.NewRow();

            table.Set(rowId, p => p.Name, "124");
            table.Set(rowId, p => p.Age, 124);
        }
    }
}
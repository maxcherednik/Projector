using System.Linq;
using Projector.Data.Filter;
using Projector.Data.GroupBy;
using Projector.Data.Join;
using Projector.Data.Projection;
using Projector.Data.Tables;
using Xunit;

namespace Projector.Data.Test
{
    public class ProjectionsTest
    {
        [Fact]
        public void CreateTableTest ()
        {
            var table = new Table<Person> ();

            Assert.Equal (2, table.Schema.Columns.Count);

            var field1 = table.Schema.Columns.Single (x => x.Name == "Name");
            Assert.Equal (typeof (string), field1.DataType);

            var field2 = table.Schema.Columns.Single (x => x.Name == "Age");
            Assert.Equal (typeof (int), field2.DataType);
        }

        [Fact]
        public void CreateFilterTest ()
        {
            var table = new Table<Person> ();
            var filteredData = table.Where (x => x.Age > 5);

            Assert.IsType<Filter<Person>> (filteredData);
        }

        [Fact]
        public void CreateProjectionTest ()
        {
            var table = new Table<Person> ();
            var projectionData = table.Projection (x => new { x.Name, ProjectedAge = x.Age * 5 });

            Assert.IsType<Projection<Person, dynamic>> (projectionData);
        }

        [Fact]
        public void CreateJoinTest ()
        {
            var leftTable = new Table<Person> ();
            var rightTable = new Table<Person> ();

            var joinedResult = leftTable.InnerJoin (rightTable, left => left.Name, right => right.Name, (left, right) => new { left.Name, left.Age, RightAge = right.Age });
            Assert.IsType<Join<Person, Person, string, dynamic>> (joinedResult);
        }

        [Fact]
        public void CreateLeftJoinTest ()
        {
            var leftTable = new Table<Person> ();
            var rightTable = new Table<Person> ();

            var joinedResult = leftTable.LeftJoin (rightTable, left => left.Name, right => right.Name, (left, right) => new { left.Name, left.Age, RightAge = right.Age });

            Assert.IsType<Join<Person, Person, string, dynamic>> (joinedResult);
        }

        [Fact]
        public void CreateRightJoinTest ()
        {
            var leftTable = new Table<Person> ();
            var rightTable = new Table<Person> ();

            var joinedResult = leftTable.RightJoin (rightTable, left => left.Name, right => right.Name, (left, right) => new { left.Name, left.Age, RightAge = right.Age });

            Assert.IsType<Join<Person, Person, string, dynamic>> (joinedResult);
        }

        [Fact]
        public void CreateGrouByTest ()
        {
            var personTable = new Table<Person> ();

            personTable.GroupBy (person => person.Name, (key, persons) => new { PersonName = key, PersonMaxAge = persons.Max (p => p.Age) });
        }

        [Fact]
        public void ProjectionChainingTest ()
        {
            var sourceTable = new Table<Person> ();

            var result = sourceTable
                .Where (p => p.Age > 25)
                .Projection (p => new { p.Age, Name1 = p.Name, NameAge = p.Name + p.Age });

            Assert.IsType<Projection<Person, dynamic>> (result);
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}

using Projector.Data.Join;
using Projector.Data.Tables;
using Projector.Data.Test.Helpers;
using Xunit;

namespace Projector.Data.Test.Join
{
    public class JoinExtensionTest
    {
        public void CreateJoinTest()
        {
            var leftTable = new Table<Person>();
            var rightTable = new Table<Person>();

            var joinedResult = leftTable.InnerJoin(rightTable, left => left.Name, right => right.Name, (left, right) => new { left.Name, left.Age, RightAge = right.Age });
            Assert.IsType<Join<Person, Person, string, dynamic>>(joinedResult);
        }

        public void CreateLeftJoinTest()
        {
            var leftTable = new Table<Person>();
            var rightTable = new Table<Person>();

            var joinedResult = leftTable.LeftJoin(rightTable, left => left.Name, right => right.Name, (left, right) => new { left.Name, left.Age, RightAge = right.Age });

            Assert.IsType<Join<Person, Person, string, dynamic>>(joinedResult);
        }

        public void CreateRightJoinTest()
        {
            var leftTable = new Table<Person>();
            var rightTable = new Table<Person>();

            var joinedResult = leftTable.RightJoin(rightTable, left => left.Name, right => right.Name, (left, right) => new { left.Name, left.Age, RightAge = right.Age });

            Assert.IsType<Join<Person, Person, string, dynamic>>(joinedResult);
        }
    }
}

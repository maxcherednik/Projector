using System.Linq;
using Projector.Data.GroupBy;
using Projector.Data.Tables;
using Xunit;

namespace Projector.Data.Test.GroupBy
{
    public class GroupByExtensionTest
    {
        [Fact]
        public void CreateGrouByTest()
        {
            var personTable = new Table<Person>();

            personTable.GroupBy(person => person.Name, (key, persons) => new { PersonName = key, PersonMaxAge = persons.Max(p => p.Age) });
        }
    }
}

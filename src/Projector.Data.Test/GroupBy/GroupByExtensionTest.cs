using System.Linq;
using NSubstitute;
using Projector.Data.GroupBy;
using Xunit;

namespace Projector.Data.Test.GroupBy
{
    public class GroupByExtensionTest
    {
        [Fact]
        public void CreateGrouByTest()
        {
            var mockDataProvider = Substitute.For<IDataProvider<Person>>();

            var groupBy = mockDataProvider.GroupBy(person => person.Name, (key, persons) => new { PersonName = key, PersonMaxAge = persons.Max(p => p.Age) });

            mockDataProvider.Received(1).AddConsumer(groupBy);
            mockDataProvider.DidNotReceive().RemoveConsumer(Arg.Any<IDataConsumer>());
        }
    }
}

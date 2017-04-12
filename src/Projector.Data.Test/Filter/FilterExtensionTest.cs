using NSubstitute;
using Projector.Data.Filter;
using Xunit;

namespace Projector.Data.Test.Filter
{
    public class FilterExtensionTest
    {
        [Fact]
        public void CreateFilterTest ()
        {
            var mockDataProvider = Substitute.For<IDataProvider<Person>> ();
            
            var filteredData = mockDataProvider.Where (x => x.Age > 5);

            mockDataProvider.Received (1).AddConsumer (filteredData);
            mockDataProvider.DidNotReceive().RemoveConsumer(Arg.Any<IDataConsumer> ());

            Assert.IsType<Filter<Person>> (filteredData);
        }
    }
}
using NSubstitute;
using Projector.Data.Projection;
using Xunit;

namespace Projector.Data.Test.Projection
{
    public class ProjectionExtensionTest
    {
        [Fact]
        public void CreateProjectionTest()
        {
            var mockDataProvider = Substitute.For<IDataProvider<Person>>();
            var projectionData = mockDataProvider.Projection(x => new { x.Name, ProjectedAge = x.Age * 5 });

            mockDataProvider.Received(1).AddConsumer(projectionData);
            mockDataProvider.DidNotReceive().RemoveConsumer(Arg.Any<IDataConsumer>());
        }
    }
}
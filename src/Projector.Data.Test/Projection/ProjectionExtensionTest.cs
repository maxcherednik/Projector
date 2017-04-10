using Projector.Data.Projection;
using Projector.Data.Tables;
using Xunit;

namespace Projector.Data.Test.Projection
{
    public class ProjectionExtensionTest
    {
        [Fact]
        public void CreateProjectionTest ()
        {
            var table = new Table<Person> ();
            var projectionData = table.Projection (x => new { x.Name, ProjectedAge = x.Age * 5 });

            Assert.IsType<Projection<Person, dynamic>> (projectionData);
        }
    }
}
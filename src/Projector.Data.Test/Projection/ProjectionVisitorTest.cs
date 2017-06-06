using NSubstitute;
using Projector.Data.Projection;
using System;
using System.Linq.Expressions;
using Projector.Data.Test.Helpers;
using Xunit;

namespace Projector.Data.Test.Projection
{
    public class ProjectionVisitorTest
    {
        private readonly ISchema _mockSchema;

        public ProjectionVisitorTest()
        {
            _mockSchema = Substitute.For<ISchema>();

            var mockAgeField = Substitute.For<IField<int>>();
            mockAgeField.GetValue(1).Returns(25);

            var mockNameField = Substitute.For<IField<string>>();
            mockNameField.GetValue(1).Returns("Max");

            _mockSchema.GetField<int>("Age").Returns(mockAgeField);

            _mockSchema.GetField<string>("Name").Returns(mockNameField);
        }

        [Fact]
        public void CreateSimpleProjectionSchemaFromExpressionTest()
        {
            //call
            Expression<Func<Person, PersonProjected>> filterExpression = person => new PersonProjected { Name = person.Name, NameAge = person.Name + person.Age };
            var projectedFieldsMeta = new ProjectionVisitor().GenerateProjection(filterExpression);
            var oldFieldNamesToNewFieldNamesMapping = projectedFieldsMeta.Item1;
            var projectedFields = projectedFieldsMeta.Item2;

            //check mappings

            Assert.Equal(2, oldFieldNamesToNewFieldNamesMapping.Count);

            Assert.True(oldFieldNamesToNewFieldNamesMapping.ContainsKey("Name"));
            Assert.True(oldFieldNamesToNewFieldNamesMapping.ContainsKey("Age"));

            var firstMapping = oldFieldNamesToNewFieldNamesMapping["Name"];
            Assert.Equal(2, firstMapping.Count);
            Assert.True(firstMapping.Contains("Name"));
            Assert.True(firstMapping.Contains("NameAge"));

            var secondMapping = oldFieldNamesToNewFieldNamesMapping["Age"];
            Assert.Equal(1, secondMapping.Count);
            Assert.True(secondMapping.Contains("NameAge"));

            // check projected fields

            Assert.Equal(2, projectedFields.Count);
            Assert.True(projectedFields.ContainsKey("Name"));
            Assert.True(projectedFields.ContainsKey("NameAge"));

            var nameField = (ProjectedField<string>)projectedFields["Name"];
            var nameAgeField = (ProjectedField<string>)projectedFields["NameAge"];

            nameField.SetSchema(_mockSchema);

            Assert.Equal("Max", nameField.GetValue(1));

            nameAgeField.SetSchema(_mockSchema);

            Assert.Equal("Max25", nameAgeField.GetValue(1));
        }
    }
}

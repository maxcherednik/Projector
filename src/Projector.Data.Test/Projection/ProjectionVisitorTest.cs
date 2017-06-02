using NSubstitute;
using Projector.Data.Projection;
using System;
using System.Linq.Expressions;
using Xunit;

namespace Projector.Data.Test.Projection
{
    public class ProjectionVisitorTest
    {
        private readonly ISchema _mockSchema;
        private readonly IField<int> _mockAgeField;
        private readonly IField<string> _mockNameField;

        public ProjectionVisitorTest()
        {
            _mockSchema = Substitute.For<ISchema>();

            _mockAgeField = Substitute.For<IField<int>>();
            _mockAgeField.GetValue(1).Returns(25);

            _mockNameField = Substitute.For<IField<string>>();
            _mockNameField.GetValue(1).Returns("Max");

            _mockSchema.GetField<int>("Age").Returns(_mockAgeField);

            _mockSchema.GetField<string>("Name").Returns(_mockNameField);
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

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        private class PersonProjected
        {
            public string Name { get; set; }

            public string NameAge { get; set; }
        }
    }
}

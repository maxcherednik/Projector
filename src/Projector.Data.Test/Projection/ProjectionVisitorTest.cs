using NSubstitute;
using Projector.Data.Projection;
using System;
using System.Linq.Expressions;
using Xunit;

namespace Projector.Data.Test.Projection
{
    public class ProjectionVisitorTest
    {
        private ISchema _mockSchema;
        private IField<int> _mockAgeField;
        private IField<string> _mockNameField;

        public ProjectionVisitorTest ()
        {
            _mockSchema = Substitute.For<ISchema> ();

            _mockAgeField = Substitute.For<IField<int>> ();
            _mockAgeField.GetValue(1).Returns (25);

            _mockNameField = Substitute.For<IField<string>> ();
            _mockNameField.GetValue(1).Returns ("Max");

            _mockSchema.GetField<int> ("Age").Returns (_mockAgeField);

            _mockSchema.GetField<string> ("Name").Returns (_mockNameField);
        }

        [Fact]
        public void CreateSimpleProjectionSchemaFromExpressionTest ()
        {
            //call
            Expression<Func<Person, PersonProjected>> filterExpression = person => new PersonProjected { Name = person.Name, NameAge = person.Name + person.Age };
            var projectedFields = new ProjectionVisitor ().GenerateProjection (filterExpression);

            //check

            Assert.Equal (2, projectedFields.Count);
            Assert.True (projectedFields.ContainsKey ("Name"));
            Assert.True (projectedFields.ContainsKey ("NameAge"));

            var nameField = (ProjectedField<string>)projectedFields ["Name"];
            var nameAgeField = (ProjectedField<string>)projectedFields ["NameAge"];

            nameField.SetSchema (_mockSchema);

            Assert.Equal ("Max", nameField.GetValue(1));

            nameAgeField.SetSchema (_mockSchema);

            Assert.Equal ("Max25", nameAgeField.GetValue(1));
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

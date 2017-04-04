using NSubstitute;
using NUnit.Framework;
using Projector.Data.Filter;
using System;
using System.Linq.Expressions;

namespace Projector.Data.Test.Filter
{
    [TestFixture]
    public class FilterVisitorTest
    {

        private ISchema _mockSchema;
        private IField<int> _mockAgeField;
        private IField<string> _mockNameField;

        [SetUp]
        public void InitContext()
        {
            _mockSchema = Substitute.For<ISchema>();

            _mockAgeField = Substitute.For<IField<int>>();
            _mockNameField = Substitute.For<IField<string>>();

            _mockSchema.GetField<int>(1, "Age").Returns(_mockAgeField);
            _mockSchema.GetField<int>(121, "Age").Returns(_mockAgeField);

            _mockSchema.GetField<string>(1, "Name").Returns(_mockNameField);
        }


        [Test]
        public void CreateSimpleFilterDelegateFromExpressionTest()
        {
            // setup
            _mockAgeField.Value.Returns(4);

            //call
            Expression<Func<Person, bool>> filterExpression = person => person.Age > 5;
            var filter = new FilterVisitor().GenerateFilter(filterExpression);

            //check
            Assert.False(filter(_mockSchema, 1));

            _mockSchema.Received(1).GetField<int>(1, "Age");
        }

        [Test]
        public void CreateComplexFilterDelegateFromExpressionTest()
        {
            // setup
            _mockAgeField.Value.Returns(6);

            // call
            Expression<Func<Person, bool>> filterExpression = person => person.Age > 5 && person.Age < 10;
            var filter = new FilterVisitor().GenerateFilter(filterExpression);

            //check
            Assert.True(filter(_mockSchema, 1));

            _mockSchema.Received(2).GetField<int>(1, "Age");


            //second round 
            _mockSchema.ClearReceivedCalls();
            _mockAgeField.Value.Returns(10);

            Assert.False(filter(_mockSchema, 121));

            _mockSchema.Received(2).GetField<int>(121, "Age");
        }


        [Test]
        public void CreateSimpleFilterForStringFieldDelegateFromExpressionTest()
        {
            // setup
            _mockNameField.Value.Returns("Max");

            //call
            Expression<Func<Person, bool>> filterExpression = person => person.Name=="Max";
            var filter = new FilterVisitor().GenerateFilter(filterExpression);

            //check
            Assert.True(filter(_mockSchema, 1));

            _mockSchema.Received(1).GetField<string>(1, "Name");
        }

        [Test]
        public void CreateSuperComplexFilterFieldDelegateFromExpressionTest()
        {
            // setup
            _mockNameField.Value.Returns("Max");
            _mockAgeField.Value.Returns(6);

            //call
            Expression<Func<Person, bool>> filterExpression = person => person.Name == "Max"  && person.Age ==6 && (person.Name + person.Age).StartsWith("M");
            var filter = new FilterVisitor().GenerateFilter(filterExpression);

            //check
            Assert.True(filter(_mockSchema, 1));

            _mockSchema.Received(2).GetField<string>(1, "Name");
            _mockSchema.Received(2).GetField<int>(1, "Age");
        }



        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}

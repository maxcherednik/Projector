using NSubstitute;
using Projector.Data.Filter;
using System;
using System.Linq.Expressions;
using Xunit;

namespace Projector.Data.Test.Filter
{
    public class FilterVisitorTest
    {
        private ISchema _mockSchema;
        private IField<int> _mockAgeField;
        private IField<string> _mockNameField;

        public FilterVisitorTest()
        {
            _mockSchema = Substitute.For<ISchema>();

            _mockAgeField = Substitute.For<IField<int>>();
            _mockNameField = Substitute.For<IField<string>>();

            _mockSchema.GetField<int>("Age").Returns(_mockAgeField);

            _mockSchema.GetField<string>("Name").Returns(_mockNameField);
        }

        [Fact]
        public void CreateSimpleFilterDelegateFromExpressionTest()
        {
            // setup
            _mockAgeField.GetValue(1).Returns(4);

            //call
            Expression<Func<Person, bool>> filterExpression = person => person.Age > 5;
            var filter = new FilterVisitor().GenerateFilter(filterExpression).Item2;

            //check
            Assert.False(filter(_mockSchema, 1));

            _mockSchema.Received(1).GetField<int>("Age");
        }

        [Fact]
        public void CreateComplexFilterDelegateFromExpressionTest()
        {
            // setup
            _mockAgeField.GetValue(1).Returns(6);

            // call
            Expression<Func<Person, bool>> filterExpression = person => person.Age > 5 && person.Age < 10;
            var filter = new FilterVisitor().GenerateFilter(filterExpression).Item2;

            //check
            Assert.True(filter(_mockSchema, 1));

            _mockSchema.Received(2).GetField<int>("Age");


            //second round 
            _mockSchema.ClearReceivedCalls();
            _mockAgeField.GetValue(121).Returns(10);

            Assert.False(filter(_mockSchema, 121));

            _mockSchema.Received(2).GetField<int>("Age");
        }

        [Fact]
        public void CreateSimpleFilterForStringFieldDelegateFromExpressionTest()
        {
            // setup
            _mockNameField.GetValue(1).Returns("Max");

            //call
            Expression<Func<Person, bool>> filterExpression = person => person.Name == "Max";
            var filter = new FilterVisitor().GenerateFilter(filterExpression).Item2;

            //check
            Assert.True(filter(_mockSchema, 1));

            _mockSchema.Received(1).GetField<string>("Name");
        }

        [Fact]
        public void CreateSuperComplexFilterFieldDelegateFromExpressionTest()
        {
            // setup
            _mockNameField.GetValue(1).Returns("Max");
            _mockAgeField.GetValue(1).Returns(6);

            //call
            Expression<Func<Person, bool>> filterExpression = person => person.Name == "Max" && person.Age == 6 && (person.Name + person.Age).StartsWith("M");
            var filter = new FilterVisitor().GenerateFilter(filterExpression).Item2;

            //check
            Assert.True(filter(_mockSchema, 1));

            _mockSchema.Received(2).GetField<string>("Name");
            _mockSchema.Received(2).GetField<int>("Age");
        }

        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}

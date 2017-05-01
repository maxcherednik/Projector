using NSubstitute;
using Projector.Data.Join;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace Projector.Data.Test.Join
{
    public class ResultSelectorVisitorTest
    {
        private ISchema _mockSchemaLeft;
        private IField<int> _mockAgeField;
        private IField<string> _mockNameField;

        private ISchema _mockSchemaRight;
        private IField<string> _mockStreetField;
        private IField<int> _mockHouseNumberField;
        private Dictionary<int, Tuple<int, int>> _rowMap;

        public ResultSelectorVisitorTest()
        {
            // left schema setup
            _mockSchemaLeft = Substitute.For<ISchema>();

            _mockAgeField = Substitute.For<IField<int>>();
            _mockAgeField.GetValue(125).Returns(25);

            _mockNameField = Substitute.For<IField<string>>();
            _mockNameField.GetValue(125).Returns("Max");

            _mockSchemaLeft.GetField<int>("Age").Returns(_mockAgeField);

            _mockSchemaLeft.GetField<string>("Name").Returns(_mockNameField);

            // right schema setup
            _mockSchemaRight = Substitute.For<ISchema>();

            _mockHouseNumberField = Substitute.For<IField<int>>();
            _mockHouseNumberField.GetValue(13).Returns(221);

            _mockStreetField = Substitute.For<IField<string>>();
            _mockStreetField.GetValue(13).Returns("Baker street");

            _mockSchemaRight.GetField<int>("HouseNumber").Returns(_mockHouseNumberField);

            _mockSchemaRight.GetField<string>("Street").Returns(_mockStreetField);

            _rowMap = new Dictionary<int, Tuple<int, int>>
            {
                { 1, Tuple.Create(125, 13) }
            };
        }

        [Fact]
        public void CreateSimpleProjectionSchemaFromExpressionTest()
        {
            //call
            Expression<Func<Person, PersonAddress, dynamic>> filterExpression = (person, personAddress) => new
            {
                Name = person.Name,
                NameAge = person.Name + person.Age,
                personAddress.Street,
                NameAddressHouse = person.Name + personAddress.Street + personAddress.HouseNumber
            };

            var projectedFieldsMeta = new ResultSelectorVisitor().GenerateProjection(filterExpression);
            var oldFieldNamesToNewFieldNamesMapping = projectedFieldsMeta.LeftSourceOldNamesToNewNamesMapping;
            var projectedFields = projectedFieldsMeta.ProjectedFields;

            //check mappings

            Assert.Equal(4, oldFieldNamesToNewFieldNamesMapping.Count);

            Assert.True(oldFieldNamesToNewFieldNamesMapping.ContainsKey("Name"));
            Assert.True(oldFieldNamesToNewFieldNamesMapping.ContainsKey("Age"));
            Assert.True(oldFieldNamesToNewFieldNamesMapping.ContainsKey("Street"));
            Assert.True(oldFieldNamesToNewFieldNamesMapping.ContainsKey("HouseNumber"));

            var firstMapping = oldFieldNamesToNewFieldNamesMapping["Name"];
            Assert.Equal(3, firstMapping.Count);
            Assert.True(firstMapping.Contains("Name"));
            Assert.True(firstMapping.Contains("NameAge"));
            Assert.True(firstMapping.Contains("NameAddressHouse"));


            var secondMapping = oldFieldNamesToNewFieldNamesMapping["Age"];
            Assert.Equal(1, secondMapping.Count);
            Assert.True(secondMapping.Contains("NameAge"));

            var thirdMapping = oldFieldNamesToNewFieldNamesMapping["Street"];
            Assert.Equal(2, thirdMapping.Count);
            Assert.True(thirdMapping.Contains("Street"));
            Assert.True(thirdMapping.Contains("NameAddressHouse"));

            var fouthMapping = oldFieldNamesToNewFieldNamesMapping["HouseNumber"];
            Assert.Equal(1, fouthMapping.Count);
            Assert.True(fouthMapping.Contains("NameAddressHouse"));

            // check projected fields

            Assert.Equal(4, projectedFields.Count);
            Assert.True(projectedFields.ContainsKey("Name"));
            Assert.True(projectedFields.ContainsKey("NameAge"));
            Assert.True(projectedFields.ContainsKey("Street"));
            Assert.True(projectedFields.ContainsKey("NameAddressHouse"));


            var nameField = (JoinProjectedField<string>)projectedFields["Name"];
            nameField.SetLeftSchema(_mockSchemaLeft);
            nameField.SetRightSchema(_mockSchemaRight);
            nameField.SetRowIdMap(_rowMap);

            Assert.Equal("Max", nameField.GetValue(1));


            var nameAgeField = (JoinProjectedField<string>)projectedFields["NameAge"];
            nameAgeField.SetLeftSchema(_mockSchemaLeft);
            nameAgeField.SetRightSchema(_mockSchemaRight);
            nameAgeField.SetRowIdMap(_rowMap);

            Assert.Equal("Max25", nameAgeField.GetValue(1));


            var streetField = (JoinProjectedField<string>)projectedFields["Street"];
            streetField.SetLeftSchema(_mockSchemaLeft);
            streetField.SetRightSchema(_mockSchemaRight);
            streetField.SetRowIdMap(_rowMap);

            Assert.Equal("Baker street", streetField.GetValue(1));


            var nameAddressHouseField = (JoinProjectedField<string>)projectedFields["NameAddressHouse"];
            nameAddressHouseField.SetLeftSchema(_mockSchemaLeft);
            nameAddressHouseField.SetRightSchema(_mockSchemaRight);
            nameAddressHouseField.SetRowIdMap(_rowMap);

            Assert.Equal("MaxBaker street221", nameAddressHouseField.GetValue(1));
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        private class PersonAddress
        {
            public string Name { get; set; }

            public string Street { get; set; }

            public int HouseNumber { get; set; }
        }

    }
}

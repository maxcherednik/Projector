﻿using NSubstitute;
using Projector.Data.Join;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace Projector.Data.Test.Join
{
    public class ResultSelectorVisitorTest
    {
        private readonly ISchema _mockSchemaLeft;
        private readonly IField<int> _mockAgeField;
        private readonly IField<string> _mockNameField;

        private readonly ISchema _mockSchemaRight;
        private readonly IField<string> _mockStreetField;
        private readonly IField<int> _mockHouseNumberField;
        private readonly Dictionary<int, RowMap> _rowMap;

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

            _rowMap = new Dictionary<int, RowMap>
            {
                { 1, new RowMap(125, 13) }
            };
        }

        [Fact]
        public void CreateSimpleProjectionSchemaFromExpressionTest()
        {
            //call
            Expression<Func<Person, PersonAddress, dynamic>> filterExpression = (person, personAddress) => new
            {
                person.Name,
                NameAge = person.Name + person.Age,
                personAddress.Street,
                NameAddressHouse = person.Name + personAddress.Street + personAddress.HouseNumber
            };

            var projectedFieldsMeta = new ResultSelectorVisitor().GenerateProjection(filterExpression);
            var oldFieldNamesToNewFieldNamesMappingLeft = projectedFieldsMeta.LeftSourceOldNamesToNewNamesMapping;
            var oldFieldNamesToNewFieldNamesMappingRight = projectedFieldsMeta.RightSourceOldNamesToNewNamesMapping;
            var projectedFields = projectedFieldsMeta.ProjectedFields;

            //check left mappings

            Assert.Equal(2, oldFieldNamesToNewFieldNamesMappingLeft.Count);

            Assert.True(oldFieldNamesToNewFieldNamesMappingLeft.ContainsKey("Name"));
            Assert.True(oldFieldNamesToNewFieldNamesMappingLeft.ContainsKey("Age"));

            var firstMapping = oldFieldNamesToNewFieldNamesMappingLeft["Name"];
            Assert.Equal(3, firstMapping.Count);
            Assert.True(firstMapping.Contains("Name"));
            Assert.True(firstMapping.Contains("NameAge"));
            Assert.True(firstMapping.Contains("NameAddressHouse"));


            var secondMapping = oldFieldNamesToNewFieldNamesMappingLeft["Age"];
            Assert.Equal(1, secondMapping.Count);
            Assert.True(secondMapping.Contains("NameAge"));

            //check right mappings

            Assert.Equal(2, oldFieldNamesToNewFieldNamesMappingRight.Count);

            var thirdMapping = oldFieldNamesToNewFieldNamesMappingRight["Street"];
            Assert.Equal(2, thirdMapping.Count);
            Assert.True(thirdMapping.Contains("Street"));
            Assert.True(thirdMapping.Contains("NameAddressHouse"));

            var fouthMapping = oldFieldNamesToNewFieldNamesMappingRight["HouseNumber"];
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

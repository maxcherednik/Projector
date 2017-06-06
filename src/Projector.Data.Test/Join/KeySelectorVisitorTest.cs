using NSubstitute;
using Projector.Data.Join;
using System;
using System.Linq.Expressions;
using Projector.Data.Test.Helpers;
using Xunit;

namespace Projector.Data.Test.Join
{
    public class KeySelectorVisitorTest
    {
        [Fact]
        public void CreateKeySelectorMetadataFromExpressionForSimpleSingleFieldKeyTest()
        {
            // setup 
            // left schema setup
            var mockSchemaLeft = Substitute.For<ISchema>();

            var mockAgeField = Substitute.For<IField<int>>();
            mockAgeField.GetValue(125).Returns(25);

            var mockNameField = Substitute.For<IField<string>>();
            mockNameField.GetValue(125).Returns("Max");

            mockSchemaLeft.GetField<int>("Age").Returns(mockAgeField);

            mockSchemaLeft.GetField<string>("Name").Returns(mockNameField);

            // right schema setup
            var mockSchemaRight = Substitute.For<ISchema>();

            var mockPersonAddressNameField = Substitute.For<IField<string>>();
            mockPersonAddressNameField.GetValue(13).Returns("Max");
            mockPersonAddressNameField.GetValue(14).Returns("Joe");

            var mockHouseNumberField = Substitute.For<IField<int>>();
            mockHouseNumberField.GetValue(13).Returns(221);

            var mockStreetField = Substitute.For<IField<string>>();
            mockStreetField.GetValue(13).Returns("Baker street");

            mockSchemaRight.GetField<string>("Name").Returns(mockPersonAddressNameField);

            mockSchemaRight.GetField<int>("HouseNumber").Returns(mockHouseNumberField);

            mockSchemaRight.GetField<string>("Street").Returns(mockStreetField);

            //call

            Expression<Func<Person, string>> leftKeySelectorExpression = person => person.Name;

            Expression<Func<PersonAddress, string>> rightKeySelectorExpression = personAddress => personAddress.Name;

            var rowMatchMeta = new KeySelectorVisitor<Person, PersonAddress, string>(leftKeySelectorExpression, rightKeySelectorExpression)
                                        .Generate();

            // check

            Assert.True(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 13)); // Max == Max

            Assert.False(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 14)); // Max != Joe

            // check left keys mappings

            Assert.Equal(1, rowMatchMeta.LeftKeyFieldNames.Count);

            Assert.True(rowMatchMeta.LeftKeyFieldNames.Contains("Name"));

            // check right keys mappings 
            Assert.Equal(1, rowMatchMeta.RightKeyFieldNames.Count);

            Assert.True(rowMatchMeta.RightKeyFieldNames.Contains("Name"));
        }

        [Fact]
        public void CreateKeySelectorMetadataFromExpressionForConstantKeyTest()
        {
            // setup 
            // left schema setup
            var mockSchemaLeft = Substitute.For<ISchema>();

            var mockAgeField = Substitute.For<IField<int>>();
            mockAgeField.GetValue(125).Returns(25);

            var mockNameField = Substitute.For<IField<string>>();
            mockNameField.GetValue(125).Returns("Max");

            mockSchemaLeft.GetField<int>("Age").Returns(mockAgeField);

            mockSchemaLeft.GetField<string>("Name").Returns(mockNameField);

            // right schema setup
            var mockSchemaRight = Substitute.For<ISchema>();

            var mockPersonAddressNameField = Substitute.For<IField<string>>();
            mockPersonAddressNameField.GetValue(13).Returns("Max");
            mockPersonAddressNameField.GetValue(14).Returns("Joe");

            var mockHouseNumberField = Substitute.For<IField<int>>();
            mockHouseNumberField.GetValue(13).Returns(221);

            var mockStreetField = Substitute.For<IField<string>>();
            mockStreetField.GetValue(13).Returns("Baker street");

            mockSchemaRight.GetField<string>("Name").Returns(mockPersonAddressNameField);

            mockSchemaRight.GetField<int>("HouseNumber").Returns(mockHouseNumberField);

            mockSchemaRight.GetField<string>("Street").Returns(mockStreetField);
            
            //call

            Expression<Func<Person, string>> leftKeySelectorExpression = person => "1";

            Expression<Func<PersonAddress, string>> rightKeySelectorExpression = personAddress => "1";

            var rowMatchMeta = new KeySelectorVisitor<Person, PersonAddress, string>(leftKeySelectorExpression, rightKeySelectorExpression)
                                        .Generate();

            // check

            Assert.True(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 13)); // rows are matching cause "1"=="1"

            Assert.True(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 14)); // rows are matching cause "1"=="1"

            // check left keys mappings

            Assert.Equal(0, rowMatchMeta.LeftKeyFieldNames.Count);

            // check right keys mappings 
            Assert.Equal(0, rowMatchMeta.RightKeyFieldNames.Count);
        }

        [Fact]
        public void CreateKeySelectorMetadataFromExpressionForCompoundKeyTest()
        {
            // setup 
            // left schema setup
            var mockSchemaLeft = Substitute.For<ISchema>();

            var mockAgeField = Substitute.For<IField<int>>();
            mockAgeField.GetValue(125).Returns(25);

            var mockNameField = Substitute.For<IField<string>>();
            mockNameField.GetValue(125).Returns("Max");

            mockSchemaLeft.GetField<int>("Age").Returns(mockAgeField);

            mockSchemaLeft.GetField<string>("Name").Returns(mockNameField);

            // right schema setup
            var mockSchemaRight = Substitute.For<ISchema>();

            var mockPersonAddressNameField = Substitute.For<IField<string>>();
            mockPersonAddressNameField.GetValue(13).Returns("Max");
            mockPersonAddressNameField.GetValue(14).Returns("Joe");

            var mockHouseNumberField = Substitute.For<IField<int>>();
            mockHouseNumberField.GetValue(13).Returns(25);
            mockHouseNumberField.GetValue(14).Returns(215);

            var mockStreetField = Substitute.For<IField<string>>();
            mockStreetField.GetValue(13).Returns("Baker street");

            mockSchemaRight.GetField<string>("Name").Returns(mockPersonAddressNameField);

            mockSchemaRight.GetField<int>("HouseNumber").Returns(mockHouseNumberField);

            mockSchemaRight.GetField<string>("Street").Returns(mockStreetField);

            //call

            Expression<Func<Person, dynamic>> leftKeySelectorExpression = person => new { person.Name, person.Age };

            Expression<Func<PersonAddress, dynamic>> rightKeySelectorExpression = personAddress => new { personAddress.Name, personAddress.HouseNumber };

            var rowMatchMeta = new KeySelectorVisitor<Person, PersonAddress, dynamic>(leftKeySelectorExpression, rightKeySelectorExpression)
                                        .Generate();

            // check

            Assert.True(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 13)); // Name:Max == Name:Max &&  Age:25==HouseNumber:25

            Assert.False(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 14)); // Name:Max != Name:Joe &&  Age:25==HouseNumber:25

            // check left keys mappings

            Assert.Equal(2, rowMatchMeta.LeftKeyFieldNames.Count);

            Assert.True(rowMatchMeta.LeftKeyFieldNames.Contains("Name"));
            Assert.True(rowMatchMeta.LeftKeyFieldNames.Contains("Age"));


            // check right keys mappings 
            Assert.Equal(2, rowMatchMeta.RightKeyFieldNames.Count);

            Assert.True(rowMatchMeta.RightKeyFieldNames.Contains("Name"));
            Assert.True(rowMatchMeta.RightKeyFieldNames.Contains("HouseNumber"));
        }

        [Fact]
        public void CreateKeySelectorMetadataFromExpressionForSuperCompoundKeyTest()
        {
            // setup 
            // left schema setup
            var mockSchemaLeft = Substitute.For<ISchema>();

            var mockAgeField = Substitute.For<IField<int>>();
            mockAgeField.GetValue(125).Returns(25);

            var mockNameField = Substitute.For<IField<string>>();
            mockNameField.GetValue(125).Returns("Max");

            mockSchemaLeft.GetField<int>("Age").Returns(mockAgeField);

            mockSchemaLeft.GetField<string>("Name").Returns(mockNameField);

            // right schema setup
            var mockSchemaRight = Substitute.For<ISchema>();

            var mockPersonAddressNameField = Substitute.For<IField<string>>();
            mockPersonAddressNameField.GetValue(13).Returns("Max");
            mockPersonAddressNameField.GetValue(14).Returns("Joe");

            var mockHouseNumberField = Substitute.For<IField<int>>();
            mockHouseNumberField.GetValue(13).Returns(25);
            mockHouseNumberField.GetValue(14).Returns(215);

            var mockStreetField = Substitute.For<IField<string>>();
            mockStreetField.GetValue(13).Returns("Baker street");

            mockSchemaRight.GetField<string>("Name").Returns(mockPersonAddressNameField);

            mockSchemaRight.GetField<int>("HouseNumber").Returns(mockHouseNumberField);

            mockSchemaRight.GetField<string>("Street").Returns(mockStreetField);

            //call

            Expression<Func<Person, dynamic>> leftKeySelectorExpression = person => new { ComplexKey = person.Name + person.Age };

            Expression<Func<PersonAddress, dynamic>> rightKeySelectorExpression = personAddress => new { ComplexKeyFromTheSecondTable = personAddress.Name + personAddress.HouseNumber };

            var rowMatchMeta = new KeySelectorVisitor<Person, PersonAddress, dynamic>(leftKeySelectorExpression, rightKeySelectorExpression)
                                        .Generate();

            // check

            Assert.True(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 13)); // Name:Max == Name:Max &&  Age:25==HouseNumber:25

            Assert.False(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 14)); // Name:Max != Name:Joe &&  Age:25==HouseNumber:25

            // check left keys mappings

            Assert.Equal(2, rowMatchMeta.LeftKeyFieldNames.Count);

            Assert.True(rowMatchMeta.LeftKeyFieldNames.Contains("Name"));
            Assert.True(rowMatchMeta.LeftKeyFieldNames.Contains("Age"));

            // check right keys mappings 
            Assert.Equal(2, rowMatchMeta.RightKeyFieldNames.Count);

            Assert.True(rowMatchMeta.RightKeyFieldNames.Contains("Name"));
            Assert.True(rowMatchMeta.RightKeyFieldNames.Contains("HouseNumber"));
        }

        [Fact]
        public void CreateKeySelectorMetadataFromExpressionForSuperCompoundKeyAndConcreteTypeTest()
        {
            // setup 
            // left schema setup
            var mockSchemaLeft = Substitute.For<ISchema>();

            var mockAgeField = Substitute.For<IField<int>>();
            mockAgeField.GetValue(125).Returns(25);

            var mockNameField = Substitute.For<IField<string>>();
            mockNameField.GetValue(125).Returns("Max");

            mockSchemaLeft.GetField<int>("Age").Returns(mockAgeField);

            mockSchemaLeft.GetField<string>("Name").Returns(mockNameField);

            // right schema setup
            var mockSchemaRight = Substitute.For<ISchema>();

            var mockPersonAddressNameField = Substitute.For<IField<string>>();
            mockPersonAddressNameField.GetValue(13).Returns("Max");
            mockPersonAddressNameField.GetValue(14).Returns("Joe");

            var mockHouseNumberField = Substitute.For<IField<int>>();
            mockHouseNumberField.GetValue(13).Returns(25);
            mockHouseNumberField.GetValue(14).Returns(215);

            var mockStreetField = Substitute.For<IField<string>>();
            mockStreetField.GetValue(13).Returns("Baker street");

            mockSchemaRight.GetField<string>("Name").Returns(mockPersonAddressNameField);

            mockSchemaRight.GetField<int>("HouseNumber").Returns(mockHouseNumberField);

            mockSchemaRight.GetField<string>("Street").Returns(mockStreetField);
            
            //call

            Expression<Func<Person, ConcreteKey>> leftKeySelectorExpression = person => new ConcreteKey { ComplexKey = person.Name + person.Age.ToString(), SomeNumber = person.Age };

            Expression<Func<PersonAddress, ConcreteKey>> rightKeySelectorExpression = personAddress => new ConcreteKey { ComplexKey = personAddress.Name + personAddress.HouseNumber, SomeNumber = personAddress.HouseNumber };

            var rowMatchMeta = new KeySelectorVisitor<Person, PersonAddress, ConcreteKey>(leftKeySelectorExpression, rightKeySelectorExpression)
                                        .Generate();

            // check

            Assert.True(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 13)); // Name:Max == Name:Max &&  Age:25==HouseNumber:25

            Assert.False(rowMatchMeta.RowMatcher(mockSchemaLeft, 125, mockSchemaRight, 14)); // Name:Max != Name:Joe &&  Age:25==HouseNumber:25

            // check left keys mappings

            Assert.Equal(2, rowMatchMeta.LeftKeyFieldNames.Count);

            Assert.True(rowMatchMeta.LeftKeyFieldNames.Contains("Name"));
            Assert.True(rowMatchMeta.LeftKeyFieldNames.Contains("Age"));

            // check right keys mappings 
            Assert.Equal(2, rowMatchMeta.RightKeyFieldNames.Count);

            Assert.True(rowMatchMeta.RightKeyFieldNames.Contains("Name"));
            Assert.True(rowMatchMeta.RightKeyFieldNames.Contains("HouseNumber"));
        }
    }
}

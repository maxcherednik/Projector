using NSubstitute;
using Projector.Data.Join;
using System;
using System.Collections.Generic;
using Xunit;

namespace Projector.Data.Test.Join
{
    public class JoinProjectedFieldTest
    {
        [Fact]
        public void CreateJoinProjectedFieldTest()
        {
            var mockFilterAccessor = Substitute.For<Func<ISchema, int, ISchema, int, long>>();
            var leftSchema = Substitute.For<ISchema>();
            var rightSchema = Substitute.For<ISchema>();
            var rowIdMap = Substitute.For<IDictionary<int, Tuple<int, int>>>();


            var joinProjectedField = new JoinProjectedField<long>("ProjectedName", mockFilterAccessor);

            joinProjectedField.SetLeftSchema(leftSchema);
            joinProjectedField.SetRightSchema(rightSchema);
            joinProjectedField.SetRowIdMap(rowIdMap);


            Assert.Equal("ProjectedName", joinProjectedField.Name);
            Assert.Equal(typeof(long), joinProjectedField.DataType);

            mockFilterAccessor(leftSchema, 10, rightSchema, 20).Returns(42);

            rowIdMap[123].Returns(Tuple.Create(10, 20));

            var fieldValue = joinProjectedField.GetValue(123);

            Assert.Equal(42, fieldValue);

            mockFilterAccessor.Received(1)(leftSchema, 10, rightSchema, 20);
        }
    }
}

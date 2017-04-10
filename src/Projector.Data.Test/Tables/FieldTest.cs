using System.Collections.Generic;
using Projector.Data.Tables;
using Xunit;

namespace Projector.Data.Test.Tables
{
    public class FieldTest
    {
        [Fact]
        public void CreateFieldTest()
        {
            var field = new Field<int>(new List<int>(), "column1");

            Assert.Equal(typeof(int), field.DataType);
            Assert.Equal("column1", field.Name);
        }

        [Fact]
        public void FieldSetValueTest()
        {
            var field = new Field<int>(new List<int>(), "column1");

            field.EnsureCapacity(1);
            field.SetCurrentRow(0);

            // before set should be default of int
            Assert.Equal(0, field.Value);

            var writabelIntField = (IWritableField<int>)field;
            writabelIntField.SetValue(123);

            Assert.Equal(123, field.Value);

            field.EnsureCapacity(2);

            // after we ensure capacity, current row and it's value should be the same
            Assert.Equal(123, field.Value);

            field.SetCurrentRow(1);

            // before set should be default of int
            Assert.Equal(0, field.Value);

            writabelIntField.CleanOldValue(0);

            field.SetCurrentRow(0);

            // after we clean it should be default of int
            Assert.Equal(0, field.Value);
        }
    }
}
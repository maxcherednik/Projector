using System;
using Projector.Data.Tables;
using Xunit;

namespace Projector.Data.Test.Tables
{
    public class SchemaTest
    {
        [Fact]
        public void CreateSchemaTest()
        {
            var schema = new Schema(0);

            schema.CreateField<int>("Column1");

            Assert.Equal(1, schema.Columns.Count);

            var field = schema.Columns[0];

            Assert.Equal(typeof(int), field.DataType);
            Assert.Equal("Column1", field.Name);
        }

        [Fact]
        public void CreateColumnWithTheSameNameShouldFail()
        {
            var schema = new Schema(0);

            var fieldName = "Column1";
            schema.CreateField<int>(fieldName);

            Assert.Throws<ArgumentException>(() => schema.CreateField<int>(fieldName));
        }

        [Fact]
        public void AddNewRowTest()
        {
            var schema = new Schema(0);

            schema.CreateField<int>("Column1");

            var firstRowId = schema.GetNewRowId();
            Assert.Equal(0, firstRowId);

            var secondRowId = schema.GetNewRowId();
            Assert.Equal(1, secondRowId);
        }

        [Fact]
        public void WhenRowRemovedRowIdShouldBeReused()
        {
            var schema = new Schema(0);

            schema.CreateField<int>("Column1");

            schema.GetNewRowId();
            schema.GetNewRowId();

            schema.Remove(1);

            var newRowId = schema.GetNewRowId();

            // id should be reused
            Assert.Equal(1, newRowId);
        }

        [Fact]
        public void WhenRowIdReusedColumnValuesShouldBeSetToDefault()
        {
            var schema = new Schema(0);

            schema.CreateField<int>("Column1");
            schema.CreateField<string>("Column2");

            schema.GetNewRowId();
            schema.GetNewRowId();

            var column1Filed = schema.GetWritableField<int>(1, "Column1");
            column1Filed.SetValue(23);

            var column2Filed = schema.GetWritableField<string>(1, "Column2");
            column2Filed.SetValue("SomeStringValue");

            schema.Remove(1);

            schema.GetNewRowId();

            // check
            column1Filed = schema.GetWritableField<int>(1, "Column1");
            Assert.Equal(0, column1Filed.Value);

            column2Filed = schema.GetWritableField<string>(1, "Column2");
            Assert.Null(column2Filed.Value);
        }

        [Fact]
        public void WhenRowRemovedTwiceItShouldFail()
        {
            var schema = new Schema(0);

            schema.CreateField<int>("Column1");

            schema.GetNewRowId();

            schema.Remove(0);

            Assert.Throws<ArgumentException>(() => schema.Remove(0));
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(-1)]
        [InlineData(1)]
        [InlineData(2)]
        public void WhenOutOfRangeRowRemovedItShouldFail(int indexToRemove)
        {
            var schema = new Schema(0);

            var fieldName = "Column1";
            schema.CreateField<int>(fieldName);

            schema.GetNewRowId();

            Assert.Throws<ArgumentOutOfRangeException>(() => schema.Remove(indexToRemove));
        }

        [Fact]
        public void WhenWritingToWritableFieldItShouldSaveValue()
        {
            var schema = new Schema(0);

            schema.CreateField<int>("Column1");

            schema.GetNewRowId();

            var column1Filed = schema.GetWritableField<int>(0, "Column1");
            column1Filed.SetValue(23);

            Assert.Equal(23, column1Filed.Value);
        }

        [Fact]
        public void WhenTypeOfTheWritebleFieldSpecifiedItShouldFail()
        {
            var schema = new Schema(0);

            schema.CreateField<int>("Column1");

            schema.GetNewRowId();

            Assert.Throws<InvalidCastException>(() => schema.GetWritableField<string>(1, "Column1"));
        }
    }
}

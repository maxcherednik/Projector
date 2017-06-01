using Projector.Data.Join;
using Projector.Data.Tables;
using Projector.Data.Test.Helpers;
using Xunit;

namespace Projector.Data.Test.Join
{
    /// <summary>
    /// Cases we need to cover here for both left and right tables:
    /// 1. Joined row
    /// 1.1 Delete of the row
    /// 1.2 Update of the non-key field
    /// 1.3 Update of the key field - join removed
    /// 1.4 Update of the key field - join recreated to another matching row
    /// 2. Non joined row
    /// 2.1 Delete of the row
    /// 2.2 Update of the non-key field
    /// 2.3 Update of the key field - join created
    /// 2.4 Update of the key field - join not created - original row stayed unmatched
    /// </summary>
    public class InnerJoinFullTest
    {
        [Fact]
        public void WhenTwoTablesAreEmptyAndWeAddRowToTheLeftThisRowShouldNotTriggerAnything()
        {
            // setup

            var personTable = new Table<Person>(10);
            var personAddressTable = new Table<PersonAddress>(3);

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            // let's have some people here
            var newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Max");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();


            // check

            // initial
            Assert.Equal(0, testConsumer.RowCount);
        }

        [Fact]
        public void WhenTwoTablesAreEmptyAndWeAddRowToTheRightThisRowShouldNotTriggerAnything()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            // let's have some people here
            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Max");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();


            // check

            // initial
            Assert.Equal(0, testConsumer.RowCount);
        }

        [Fact]
        public void WhenLeftTableContainsRowsAndWeAddNonMatchingRowToTheRightThisRowShouldNotTriggerAnything()
        {
            // setup

            var personTable = new Table<Person>(10);

            var newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Max");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var personAddressTable = new Table<PersonAddress>(3);

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            // let's have some people here
            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();


            // check

            // initial
            Assert.Equal(0, testConsumer.RowCount);
        }

        [Fact]
        public void WhenRightTableContainsRowsAndWeAddNonMatchingRowToTheLeftThisRowShouldNotTriggerAnything()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Max");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();


            // check

            // initial
            Assert.Equal(0, testConsumer.RowCount);
        }

        [Fact]
        public void WhenLeftTableContainsRowsAndWeAddMatchingRowToTheRightThisRowShouldTriggerJoinedRow()
        {
            // setup

            var personTable = new Table<Person>(10);

            var newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Max");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var personAddressTable = new Table<PersonAddress>(3);

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Max");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();


            // check

            // initial
            Assert.Equal(1, testConsumer.RowCount);
        }

        [Fact]
        public void WhenRightTableContainsRowsAndWeAddMatchingRowToTheLeftThisRowShouldTriggerJoinedRow()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();


            // check

            // initial
            Assert.Equal(1, testConsumer.RowCount);
        }

        [Fact]
        public void WhenThereIsJoinedRowAndWeRemoveLeftMatchingRowJoinedRowShouldBeRemoved()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            personTable.RemoveRow(0);

            personTable.FireChanges();

            // check

            // initial
            Assert.Equal(0, testConsumer.RowCount);
        }

        [Fact]
        public void WhenThereIsJoinedRowAndWeRemoveRightMatchingRowJoinedRowShouldBeRemoved()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            personAddressTable.RemoveRow(0);

            personAddressTable.FireChanges();

            // check

            // initial
            Assert.Equal(0, testConsumer.RowCount);
        }

        [Fact]
        public void WhenThereIsJoinedRowAndWeUpdateLeftNonSelectedFieldNothingShouldBeTriggered()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            personTable.Set(0, p => p.AnotherFieldWhichIsNotInTheResultFieldSet, 146);
            personTable.FireChanges();

            // check

            // initial
            Assert.Equal(0, testConsumer.UpdatedRowCount);
        }

        [Fact]
        public void WhenThereIsJoinedRowAndWeUpdateRightNonSelectedFieldNothingShouldBeTriggered()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            personAddressTable.Set(0, pa=> pa.FieldWhichIsNotInTheResultFieldSet, 145);
            personAddressTable.FireChanges();

            // check

            // initial
            Assert.Equal(0, testConsumer.UpdatedRowCount);
        }

        [Fact]
        public void WhenThereIsJoinedRowAndWeUpdateLeftSelectedFieldNothingShouldBeTriggered()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            personTable.Set(0, p => p.Age, 146);
            personTable.FireChanges();

            // check

            // initial
            Assert.Equal(1, testConsumer.UpdatedRowCount);
        }

        [Fact]
        public void WhenThereIsJoinedRowAndWeUpdateRightSelectedFieldNothingShouldBeTriggered()
        {
            // setup

            var personTable = new Table<Person>(10);

            var personAddressTable = new Table<PersonAddress>(3);

            var newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 33);

            personAddressTable.FireChanges();

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 33);

            personTable.FireChanges();

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            personAddressTable.Set(0, pa => pa.HouseNumber, 145);
            personAddressTable.FireChanges();

            // check

            // initial
            Assert.Equal(1, testConsumer.UpdatedRowCount);
        }

        [Fact]
        public void TwoTablesInnerJoinTest()
        {
            // setup

            var personTable = new Table<Person>(10);
            var personAddressTable = new Table<PersonAddress>(3);

            var join = personTable.InnerJoin(personAddressTable, person => person.Name, personAddress => personAddress.Name, (person, personAddress) => new { person.Name, person.Age, personAddress.Street, personAddress.HouseNumber });

            var testConsumer = new TestConsumer();

            join.AddConsumer(testConsumer);


            // call

            // let's have some people here
            var newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Max");
            personTable.Set(newRowId, p => p.Age, 33);

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 23);

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Anna");
            personTable.Set(newRowId, p => p.Age, 26);

            personTable.FireChanges();

            // let's put some addresses

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Max");
            personAddressTable.Set(newRowId, pa => pa.Street, "Baker street");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 221);
            personAddressTable.Set(newRowId, pa => pa.FieldWhichIsNotInTheResultFieldSet, 222321);


            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa=> pa.Street, "Oxford street");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 10);
            personAddressTable.Set(newRowId, pa=> pa.FieldWhichIsNotInTheResultFieldSet, 2221);

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Joe");
            personAddressTable.Set(newRowId, pa=> pa.Street, "Red square");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 1);
            personAddressTable.Set(newRowId, pa=> pa.FieldWhichIsNotInTheResultFieldSet, 442321);

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Anna");
            personAddressTable.Set(newRowId, pa=> pa.Street, "Choo street");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 35);
            personAddressTable.Set(newRowId, pa=> pa.FieldWhichIsNotInTheResultFieldSet, 22621);

            personAddressTable.FireChanges();
            // check

            // initial
            Assert.Equal(4, testConsumer.RowCount);


            // delete

            personTable.RemoveRow(1); // remove Joe
            personTable.FireChanges();

            Assert.Equal(2, testConsumer.RowCount);

            // lets add one more Person and Joe back

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Angela");
            personTable.Set(newRowId, p => p.Age, 21);

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, p => p.Name, "Joe");
            personTable.Set(newRowId, p => p.Age, 25); // different age
            personTable.FireChanges();

            Assert.Equal(4, testConsumer.RowCount);

            // let's add Angela's home

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, p => p.Name, "Angela");
            personAddressTable.Set(newRowId, pa=> pa.Street, "Chelsey street");
            personAddressTable.Set(newRowId, pa => pa.HouseNumber, 145);

            personAddressTable.FireChanges();

            Assert.Equal(5, testConsumer.RowCount);

            // let's update Joe's age

            // we just know that Joe is row id 3 right now
            personTable.Set(3, p => p.Age, 27); // he got older
            personTable.FireChanges();

            Assert.Equal(2, testConsumer.UpdatedRowCount);

            // let's change the field which is not in the result field
            testConsumer.CleanCallsCounter();
            personAddressTable.Set(0, pa=> pa.FieldWhichIsNotInTheResultFieldSet, 145);
            personAddressTable.FireChanges();

            Assert.Equal(0, testConsumer.CallsReceived);
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }

            public int AnotherFieldWhichIsNotInTheResultFieldSet { get; set; }
        }

        private class PersonAddress
        {
            public string Name { get; set; }

            public string Street { get; set; }

            public int HouseNumber { get; set; }

            public int FieldWhichIsNotInTheResultFieldSet { get; set; }
        }
    }
}

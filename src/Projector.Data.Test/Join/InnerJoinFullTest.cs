using Projector.Data.Join;
using Projector.Data.Tables;
using Projector.Data.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Projector.Data.Test.Join
{
    public class InnerJoinFullTest
    {
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
            personTable.Set(newRowId, "Name", "Max");
            personTable.Set(newRowId, "Age", 33);

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, "Name", "Joe");
            personTable.Set(newRowId, "Age", 23);

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, "Name", "Anna");
            personTable.Set(newRowId, "Age", 26);

            personTable.FireChanges();

            // let's put some addresses

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, "Name", "Max");
            personAddressTable.Set(newRowId, "Street", "Baker street");
            personAddressTable.Set(newRowId, "HouseNumber", 221);
            personAddressTable.Set(newRowId, "FieldWhichIsNotInTheResultFieldSet", 222321);


            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, "Name", "Joe");
            personAddressTable.Set(newRowId, "Street", "Oxford street");
            personAddressTable.Set(newRowId, "HouseNumber", 10);
            personAddressTable.Set(newRowId, "FieldWhichIsNotInTheResultFieldSet", 2221);

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, "Name", "Joe");
            personAddressTable.Set(newRowId, "Street", "Red square");
            personAddressTable.Set(newRowId, "HouseNumber", 1);
            personAddressTable.Set(newRowId, "FieldWhichIsNotInTheResultFieldSet", 442321);

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, "Name", "Anna");
            personAddressTable.Set(newRowId, "Street", "Choo street");
            personAddressTable.Set(newRowId, "HouseNumber", 35);
            personAddressTable.Set(newRowId, "FieldWhichIsNotInTheResultFieldSet", 22621);

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
            personTable.Set(newRowId, "Name", "Angela");
            personTable.Set(newRowId, "Age", 21);

            newRowId = personTable.NewRow();
            personTable.Set(newRowId, "Name", "Joe");
            personTable.Set(newRowId, "Age", 25); // different age
            personTable.FireChanges();

            Assert.Equal(4, testConsumer.RowCount);

            // let's add Angela's home

            newRowId = personAddressTable.NewRow();
            personAddressTable.Set(newRowId, "Name", "Angela");
            personAddressTable.Set(newRowId, "Street", "Chelsey street");
            personAddressTable.Set(newRowId, "HouseNumber", 145);

            personAddressTable.FireChanges();

            Assert.Equal(5, testConsumer.RowCount);

            // let's update Joe's age

            // we just know that Joe is row id 3 right now
            personTable.Set(3, "Age", 27); // he got older
            personTable.FireChanges();

            Assert.Equal(2, testConsumer.UpdatedRowCount);

            // let's change the field which is not in the result field
            testConsumer.CleanCallsCounter();
            personAddressTable.Set(0, "FieldWhichIsNotInTheResultFieldSet", 145);
            personAddressTable.FireChanges();

            Assert.Equal(0, testConsumer.CallsReceived);
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

            public int FieldWhichIsNotInTheResultFieldSet { get; set; }
        }
    }
}

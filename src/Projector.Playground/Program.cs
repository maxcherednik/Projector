using Projector.Data.Tables;
using System;
using Projector.Data.Join;
using Projector.Data.Projection;
using System.Diagnostics;
using Projector.Data.Filter;

namespace Projector.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var elementCount = 10000000;

            var personTable = new Table<Person>(elementCount);

            var personAddressTable = new Table<PersonAddress>();

            personTable.Where(person => person.Name.StartsWith("Max", StringComparison.Ordinal) && person.Age < 5)
                            .InnerJoin(personAddressTable, p => p.Name,
                                                    pa => pa.Name1,
                                                    (p, pa) => new { p.Name, p.Age, p.Time, p.Time1, pa.Name1, pa.HouseNumber })
                       
                       .Select(person => new
                       {
                           Age1 = person.Age,
                           AgeTime = person.Time.ToString() + person.Age.ToString(),
                           person.Time1,
                           SomeOtherField = person.Age < 1 ? "True" : "None",
                           person.HouseNumber
                       })
                       .AddConsumer(new ConsoleConsumer());





            Console.WriteLine("------------------------Add-----------------");
            // add
            for (int i = 0; i < elementCount; i++)
            {
                var rowId1 = personTable.NewRow();
                personTable.Set(rowId1, "Age", i);
                personTable.Set(rowId1, "Name", "Max");
                personTable.Set<long>(rowId1, "Time", 125000 + i);
                personTable.Set<long>(rowId1, "Time1", i);
            }

            personTable.FireChanges();

            var rowId = personAddressTable.NewRow();
            personAddressTable.Set(rowId, "Name1", "Max");
            personAddressTable.Set(rowId, "HouseNumber", 2444);

            rowId = personAddressTable.NewRow();
            personAddressTable.Set(rowId, "Name1", "Anna");
            personAddressTable.Set(rowId, "HouseNumber", 342);

            personAddressTable.FireChanges();

            Console.WriteLine("------------------------Delete-----------------");
            // delete
            personTable.RemoveRow(0);
            //personTable.RemoveRow (6);


            personTable.FireChanges();

            Console.WriteLine("------------------------Update-----------------");
            var age = 0;
            Console.WriteLine(" Press Enter key to continue...");
            while (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                var stopwatch = Stopwatch.StartNew();
                // update
                //for (int i = 1; i < 1000; i++)
                //{
                //    personTable.Set<long>(i, "Time", i * age);
                //    personTable.Set<long>(i, "Time1", i * age);
                //    personTable.Set<int>(i, "Age", age);
                //}


                //personTable.FireChanges();

                //personAddressTable.Set(0, "SomeNotNeededField", age);
                personAddressTable.Set(0, "HouseNumber", age);
                personAddressTable.FireChanges();

                stopwatch.Stop();
                Console.WriteLine(stopwatch.Elapsed + " Press Enter key to continue...");
                age++;
            }


            Console.WriteLine("Finished. Press any key to close...");
            Console.ReadKey();
        }


    }
}

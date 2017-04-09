using Projector.Data.Tables;
using System;
using Projector.Data.Filter;
using Projector.Data.Projection;

namespace Projector.Playground
{
    class Program
    {
        static void Main (string [] args)
        {
            var elementCount = 10;

            var personTable = new Table<Person> (elementCount);

            personTable
                       .Where (person => person.Name.StartsWith ("Max", StringComparison.Ordinal))
                       .Where (person => person.Age > 5)
                       .Projection (person => new { Age1 = person.Age })
                       .AddConsumer (new ConsoleConsumer ());

            for (int i = 0; i < elementCount; i++) {
                var rowId1 = personTable.NewRow ();
                personTable.Set (rowId1, "Age", i);
                personTable.Set (rowId1, "Name", "Max" + i);
                personTable.Set<long> (rowId1, "Time", 125000 + i);
                personTable.Set<long> (rowId1, "Time1", i);
            }

            Console.WriteLine ("Added " + elementCount);

            personTable.FireChanges ();

            personTable.RemoveRow (0);
            personTable.RemoveRow (6);
            //personTable.RemoveRow (6);

            personTable.FireChanges ();

            Console.WriteLine ("Published");


            Console.WriteLine ("Finished. Press any key to close...");
            Console.ReadKey ();
        }


    }
}

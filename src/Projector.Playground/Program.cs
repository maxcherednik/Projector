﻿using Projector.Data.Tables;
using System;
using Projector.Data.Join;
using Projector.Data.Projection;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Projector.Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int elementCount = 1000000;

            var personTable = new Table<Person>(elementCount);

            var personAddressTable = new Table<PersonAddress>();

            personTable
                //.Where(person => person.Name.StartsWith("Max", StringComparison.Ordinal) && person.Age < 5)
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
                       .AddConsumer(new ConsoleConsumer(true));


            Console.WriteLine("------------------------Add-----------------");
            // add
            for (var i = 0; i < elementCount; i++)
            {
                var rowId1 = personTable.NewRow();
                personTable.Set(rowId1, p => p.Age, i);
                personTable.Set(rowId1, p => p.Name, "Max");
                personTable.Set(rowId1, p => p.Time, 125000 + i);
                personTable.Set(rowId1, p => p.Time1, i);

                if (i % 100 == 0)
                {
                    personTable.FireChanges();
                }
            }


            var rowId = personAddressTable.NewRow();
            personAddressTable.Set(rowId, pa => pa.Name1, "Max");
            personAddressTable.Set(rowId, pa => pa.HouseNumber, 2444);

            rowId = personAddressTable.NewRow();
            personAddressTable.Set(rowId, pa => pa.Name1, "Anna");
            personAddressTable.Set(rowId, pa => pa.HouseNumber, 342);

            personAddressTable.FireChanges();

            var cancelTokenSource = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                var rnd = new Random();
                Console.WriteLine("------------------------Update-----------------");
                var age = 0;
                Console.WriteLine(" Press Enter key to continue...");
                while (!cancelTokenSource.Token.IsCancellationRequested)
                {
                    var stopwatch = Stopwatch.StartNew();

                    for (var i = 0; i < 1000; i++)
                    {
                        var rndRowId = rnd.Next(0, elementCount);

                        personTable.Set(rndRowId, p => p.Age, age);
                    }

                    personTable.FireChanges();

                    stopwatch.Stop();
                    Console.WriteLine(stopwatch.Elapsed + " Press Enter key to continue...");
                    age++;

                    Thread.Sleep(100);
                }
            }, cancelTokenSource.Token);


            Console.WriteLine("Finished. Press any key to close...");
            Console.ReadKey();

            cancelTokenSource.Cancel();

            task.Wait();
        }


    }
}

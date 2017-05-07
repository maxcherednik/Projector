Projector
=========

[![Build status](https://ci.appveyor.com/api/projects/status/gw3h61v3n8q9b95r/branch/master?svg=true)](https://ci.appveyor.com/project/maxcherednik/projector/branch/master)


### Why we are here
Projector is a linq expression based streaming library.

We all use linq expressions lots of times a day.

Let's have a look:
```C#
public class Person
{
    public string Name { get;}
    
    public string LastName { get;}
    
    public int Age { get;}
}
```

```C#
var people = new List<Person>();

var processedPeople = people.Where(person => person.LastName.StartsWith("B") && person.Age < 25)
                            .Select(person => new 
                                    { 
                                        FullName = person.Name + " " + person.LastName, 
                                        person.Age 
                                    })
                            .ToList();
```

Nothing to comment on - the code is pretty simple. Since the day one this approach took hearts of the developers with its convenient easy to write and read way of processing data. 

Now imagine a situation when you have a collection of people in memory, let's say, 1 mln people of different names and ages.
The code above is executed and we get a new list of people who satisfy the Where clause we wrote. Then this collection is sent to a subsystem which is interested in such information. So far so good.

Now the complicated part begins.
Some time later some people are removed from the initial collection, some got older.
We need to notify the subsystem that the collection we sent is not up to date anymore.

What options do we have?
- Execute the code above again, get a new collection and send it again

Good choice! Keep it simple!

What if I told you this kind of change happens every 30 seconds, 15 seconds, 1 seconds.
We would see a picture like this:

![GC pressure](https://github.com/maxcherednik/Projector/blob/readmeupdate/documentation/gc_pressure.png)

Doing so we might end up just cleaning and not doing anything useful.

What else do we have?
- Let's organize our code somehow that only diffs were sent

Smart!

Let's try to use ObservableCollection\<Person\> instead of original List\<Person\>
This collection seems to have all the event to notify us if something changes.
Wait a second! Where are all the nice linq extensions which we can attach to the ObservableCollection?

Unfortunately, they are not implemented. No more linq magic here:( We need to implement this streaming chains ourselves.

#### Here we are coming to a place where Projector shines

Look at the code:

```C#
var people = new Table<Person>();

people.Where(person => person.LastName.StartsWith("B") && person.Age < 25)
        .Select(person => new 
                { 
                    FullName = person.Name + " " + person.LastName, 
                    person.Age 
                })
        .ToConsole();
```

Looks pretty much like the original example. Of course, this was the original idea - to have a very familiar syntax and at the same time a benefit of push-based approach. 

So let's see what has changed:
1. Instead of List\<Person\> we have the Projector's collection Table\<Person\>
2. The result(and all the future updates) is printed out to the console

And here is the memory footprint from the same test as was done with the original example:

![GC pressure](https://github.com/maxcherednik/Projector/blob/readmeupdate/documentation/projector_memory.png)

A lot more information and documentation are available in the [wiki](https://github.com/maxcherednik/Projector/wiki).

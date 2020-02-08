# Common definitions

## The heap
The heap (less known also as the Free Store) is an area of memory used for
dynamically allocated objects. The free store is a better name because it does not suggest
any internal structure but rather a purpose

## The Mutator

Mutator as an entity that is responsible for executing
application code.We can define the Mutator as everything that
has the possibility to modify memory, either by modifying existing objects or by creating
new ones. Although it is not strict, additionally, we can extend it to everything that can
read memory (as reading is a crucial operation for program execution).

Mutator needs to provide the running
application three kind of operations:
* New(amount) - allocate a given amount of memory, which then will
be used by a newly created object. Please note that at this abstraction
level, we are not considering an object’s type information, which
may be or not be available from runtime. We are just providing the
required size of the memory to be allocated.
* Write(address, value) - write a specified value under a given
address. Here we also abstract whether we are considering an object
field (in object-oriented programming), global variable, or any other
kind of data organization.
* Read(address) - read a value from the specified address.

But in the world of automated garbage collection, those three operations are
places when Mutator cooperates with the garbage collector (Collector) and allocation
mechanism (Allocator). How this cooperation looks and how much it disturbs the
simplicity of the above implementations is one of the most important design concerns.
The most common enhancement we will meet in this book is adding a so-called barrier -
either it will be a read barrier or a write barrier. A barrier is a way of augmenting an
additional operation before or after particular operations. Barriers let us synchronize
(directly or indirectly, synchronously or asynchronously) with the garbage collector
mechanism to inform about the execution of the program and the memory usage
In the everyday reality of developers, the most often implementation of the Mutator
abstraction is a well-known thread.
Mutators do not have to be implemented as threads in the terms of the operating
system threads. The popular example can be Erlang ecosystem with its processes - they
are managed as super lightweight co-routines living in the runtime itself. They can be
seen as so-called “green threads,” but in the terms of Erlang VM it is better to call them
“green processes” as the separation enforced by runtime is much stronger than between
thread-like entities. This means they are entities managed on the runtime level, not the
operating system level. Another common implementation of Mutator could be based on
so-called fibers, lightweight units of execution implemented both in Linux and Windows.

## The Allocator

Allocator is an entity responsible for managing dynamic memory allocation and deallocation.

Allocator must provide two main operations:
* Allocator.Allocate(amount) - allocates a specified amount of
memory. This can be obviously extended by methods able to allocate
memory for a specific type of object if type information is available
for Allocator. As we have seen, this is internally used by Mutator.New
operation.
* Allocator.Deallocate(address) - frees a memory under a given
address to be available for future allocations. Please note that in
case of automatic memory management, this method is internal
and not exposed to the Mutator (and hence, no user code can call it
explicitly).

There a lot of different aspects of Allocator design. And
as always, in fact, all is about trade-offs, mainly between performance, implementation
complexity (which leads directly to maintainability), and others. We will dig into the two
most popular kinds of allocators: *sequential* and *free-list*.

## The Collector

the Collector as an entity that runs garbage collection (automatic memory reclaiming) code. In other words, we can see a Collector as a piece
of software (code) or thread executing it, or both. It depends on the context.

An ideal Collector would know the liveness of the object - live objects are
those which will be needed. In opposite - dead (or garbage) objects are not going to be
used and can be destroyed. Obviously, therefore commonly Collector is called Garbage
Collector or GC in short.

Because knowing a liveness of an object is impossible,8 Collector is based on a
less strict property of the object - whether it is reachable by any Mutator. Reachability
of an object means that there is a sequence of references (starting from any Mutator’s
accessible memory) between objects that eventually leads to that object

Mutator’s starting points in terms of reachability are called roots. What they exactly
are depends on specific Mutator implementation. But in most common cases, where a
Mutator is simply a thread (represented by operating system-based native thread), roots
can be:
* local variables and subroutine arguments - placed on stack or stored
in registers.
* statically allocated objects (e.g., global variables) - placed on the
heap.
* other internal data structures stored inside Collector itself.


## Reference Counting

One of the two most popular methods of automatic memory management is called
Reference Counting. The idea behind it is very simple. It is based on counting the
number of references to an object. Every object has its own reference counter.
When an object is being assigned to a variable or a field - the number of references to it is being
increased. At the same time, the reference counter of the object to which this variable
was previously indicated decreases.
The liveness of objects in the reference counting approach is being tracked by
the number of objects referencing a referent. If the counter drops to zero, no one is
referencing an object and thus it can be deallocated. But what if the counter does not
drop to zero? This says nothing about the liveness of an object - it says only that someone
is keeping a reference to it, not that it will use it. Thus, reference counting is yet another
less strict way of guessing liveness of an object.

If we return to the reachability property mentioned before, one can say that reference
counting is approximating liveness by local references and does not track a global state
of an object graph of references. In particular, without any additional improvements, it
can be mistaken by circular references. Such can be found in popular data structures like
double-linked lists. In such a case, the reference counter never drops
to zero as the data structure with value1 and data structure with value2 points to each
other.

One very big advantage and source of reference counting popularity is the fact it does
not require any runtime support. It can be implemented as an additional mechanism
for some specific types in the form of external library. It means that we can leave original
Mutator.New and Mutator.Write intact and just introduce higher-level counterparts
of such logic like classes with properly overloaded operators and constructors. For
example, this is exactly the case with the most popular C++ implementations.

A brief summary of the drawbacks and advantages of reference counting is as follows:
Advantages:
* Deterministic deallocation moment - we know that deallocation will
happen when an object’s reference counter will drop to zero. Therefore,
as long as it is no longer needed, the memory will be reclaimed.
* Less memory constraint - as memory is reclaimed as fast as objects
are no longer used, there is no overhead of memory consumed by the
objects waiting to be collected.
* Can be implemented without any support from the runtime.

Disadvantages:
* Such a naive implementation as at Listing 1-8 introduces very big
overhead on Mutator.
* Multithreading operations on reference counters require well-thought
synchronization, which can introduce additional overhead.
* Without any additional enhancements, circular references cannot be
reclaimed.

There are improvements to naive Reference Counting algorithms like **Deferred
Reference Counting or Coalesced Reference Counting**, which eliminate some of these
problems at the expense of some of the advantages (mainly immediate reclamation of
memory).

## Tracking Collector

*Tracking Garbage Collector* is based on knowledge of global context of an object’s
lifetime and can make a better decision whether it is good time to delete an object
(reclaim memory). It is, in fact, such a popular approach that almost certainly when
someone says something about Garbage Collector, he probably means Tracking Garbage
Collector. We can encounter it in runtimes like .NET, different JVM implementations,
and so on.

The core concept is that Tracking Garbage Collector finds true reachability of an
object by starting from the Mutator’s roots and recursively tracks the whole object’s
graph of a program. This is obviously not a trivial task because process memory can
take several GB and tracking all interobject references in such big volumes of data can
be difficult, especially while Mutators are running and changing all those references all
the time. The most typical approach of Tracing Garbage Collector consists of two main
steps:
* Mark - during this step Collector determines which objects in
memory can be collected by finding their reachability.
* Collect - during this step Collector reclaims memory of objects that
were found to not be longer reachable.

### Mark Phase

During the Mark step Collector determines which objects in memory should be
collected by finding their reachability. Starting from Mutator’s roots, Collector travels
through the whole objects graph and marks those which were visited. Those objects
that are not marked at the end of Mark phase are not reachable. Thanks to an object’s
marking, there is no problem with cyclic references. If during the graph’s traversing we
will get back to a previously visited object, we break further traversing because the object
is already marked.
Starting from the roots, we travel inside object’s graph through interobjects references. It is an implementation
detail whether we are visiting this graph in a depth-first or breadth-first manner.
Obviously traversing such a graph is hard during normal Mutator’s work as the
graph is changing constantly due to normal program execution - creating new objects,
variables, object’s field assignments, and so on. Therefore, in some Garbage Collector
implementations all Mutators are simply stopped for the duration of Mark phase. This
allows for a safe and consistent traverse of the graph. Of course, as soon as the threads
resume operation, the knowledge that Collector holds based on the object graph
becomes obsolete. But this is not a problem for non-reachable objects - if they were not
reachable before, they never become reachable again. However, there are many Garbage
Collector implementations where the Mark phase is done in a concurrent flavor, so
the marking process can be run alongside with the Mutator’s code. This is the case for
popular algorithms like CMS in JVM (Concurrent Mark Sweep), G1 in JVM, and in .NET
itself.

### Collect Phase
After Tracking Garbage Collector has found reachable objects, it can reclaim memory
from all the other dead objects. Collectors’ Collect phase can be designed in many
different ways due to many different aspects.

#### Sweep
In this approach, dead objects are simply marked as a free space that can be later reused.
This can be a very fast operation because (in exemplary implementation) only a single bit
mark of a memory block must be changed.
Then, in naive implementation, during allocation the memory is being scanned for
the gap size not less than the object’s size to be created.
But nontrivial implementations may need to build data structures storing
information about free blocks of memory for faster retrieval, typically in a form of a so-called
free-list.
Moreover, those free-lists must be smart enough
to merge adjacent free blocks of memory. Further optimization may lead to storing a
set of free-lists for memory gaps of ranging size. In terms of implementation details,
there are also different ways of how such a list can be scanned. Two of the most popular
approaches are best-fit and first-fit methods. In the first-fit method, we stop free-list scan
as fast as any suitable free memory block has been found. In the best-fit approach, we
always scan all free-list entries trying to find the best match of the required size.
Although quite fast, the Sweep approach has one major drawback - it eventually
leads to bigger or smaller memory fragmentation.

#### Compact

 In this approach, fragmentation is eliminated at the expense of lower performance
 because it requires moving around objects in memory. Objects are moved in a way that
 reduces the gap created after the deleted objects. Here two main different approaches
 can be further distinguished.
 In a simpler way, from an implementation point of view, Copying Compacting all live
 (reachable) objects are copied to the different region of memory each time collections
 occurs. Compacting is a simple consequence of copying each live
object one after another, omitting those no longer needed. Obviously, this induces high
memory traffic as all live objects have to be copied back and forth. It also puts a bigger
memory overhead because we have to maintain twice more memory than normally
would be needed.
brief summary of the drawbacks and advantages of Tracking Garbage Collector is
as follows:
Advantages:
* Complete transparency from the developer’s perspective - a memory
is just abstracted as would be infinite, without having to worry about
freeing memory of no longer needed objects.
* No problems with circular references.
* No big overhead on Mutators.
Disadvantages:
* More complicated implementation.
* Non-deterministic freeing objects - they will be released after some
time not being reachable.
* Stop the world needed for Mark phase - but only in a non-concurrent
flavor.
* Bigger memory constraint - as objects are not reclaimed as fast after
not being needed, more memory pressure can be introduced (more
garbage lives for some period of time).

# CPU

The processor is compatible with
the so-called Instruction Set Architecture (ISA) - it defines, among others, the set of
operations that can be executed (instructions), registers and their meaning, how
memory is addressed, and so on. In this sense, ISA is a contract (interface) established
between the processor manufacturer and its users - programs written under a given
contract.

## CPU cache

This cache is transparent from the ISA point of view. Neither the programmer (nor
even the operating system) does not necessarily need to know about its existence. They
do not have to manage it. In an ideal world, proper use and management of the cache
should be the sole responsibility of the CPU.
Because as a cache we want to use as fast-as-possible memory, the previously
mentioned SRAM chips are used. Due to the cost and the size (which takes up precious
space in the processor) resulting from this technology, they obviously cannot have as
large capacities as the main RAM.

The idea behind a cache is trivial. When the instruction executed by the processor
needs access to memory (whether it is write or read), it first looks at the cache to check
whether the data we need is there already. If so, fantastic! We have just gained a very fast
memory access and such a situation is referred to as cache hit. If the data is not in the
cache (so-called cache miss), then it is being stored there after reading from RAM, which
is obviously a much slower operation. Cache hit ratio and cache miss ratio are the very
important indicators telling us whether our code uses the cache efficiently.

### Data Locality

Cache idea is based on the very
important concept - locality of data. We can distinguish two kinds of locality:
* temporal locality - if we access some memory region, we will most
probably access it again in the near future. This makes using a
cache perfectly valid - we read some data from memory and we will
probably reuse it later a few more times. Why is there a temporal
locality? This is quite intuitive. We rarely use data once. In general,
we load some data structures into variables and use those variables
repeatedly. These are all kinds of counters, temporary data read from
files, and so on.
* spatial locality - if we access some memory region, we will most
probably access data from the close neighborhood. This type of
locality can become our ally if we cache a little more surrounding
data than we currently need. For example, if we need a few bytes
from memory, let’s read and cache them and a dozen or so more.
This is also perfectly intuitive. We rarely use very isolated small areas
of memory. We soon will find out the stack and heap are organized
into segments so threads doing their job generally access similar
areas of memory. Local variables or data structures are also generally
placed close together.


The most important and most influential fact is that the data between the RAM and
the cache is transferred in blocks called cache line. Cache line has a fixed size and in the
vast majority of today computers it is 64 bytes. It is very important to remember - you
cannot read or write less data from memory than the cache line size, so 64 bytes.Even if
 you would want to read one single bit from memory, the whole block of a 64-bytes wide
 cache line will be populated. Such a design is utilizing better sequential DRAM access

As stated before, DRAM access is 64-bit wide (8 bytes), so eight transfers are required
from RAM to populate such cache line. This requires quite a lot of CPU cycles so there
are various techniques to accommodate that. One of them is Critical Word First & Early
Restart. It makes the cache line not read word by word but starts with the word that is
most needed. Imagine that in the worst case, such an 8-byte word could be at the end of
the cache line so you would have to wait for all the previous seven transfers to access it.
This technique first reads the most important word. Instructions waiting for this data can
continue execution and the rest of the cache line will be filled asynchronously.

ow does a typical memory access pattern look? When someone wants
to read data from memory, the corresponding cache line entry is created in the
cache and 64 bytes of data are being read into it. When someone wants to write
data in memory, the first step is exactly the same – the cache line is being filled
in the cache if it is not there already. This cached data is modified in cache when
someone writes data. Now one of two strategies can occur:
* write-through - after writing to the cache line, the modified data is saved
immediately to the main memory. This is a simple approach to implement but
creates a big overhead on the memory bus.
* write-back - after writing to the cache line it is marked dirty. Then, when there
is no space in cache for other data, this dirty block is written to memory (and the
modified dirty cache entry is deleted). The processor may write these blocks from
time to time, as it deems appropriate (e.g., during idle times).
There is yet another one optimization technique called write-combining. It ensures
that a given cache line from a given memory area is written in its entirety (rather
than writing its individual words), again utilizing the fact of faster sequential access
to memory.

Because of cache lines, each data stored in memory is aligned to 64-bytes
boundaries. So in the worst-case scenario to read two successive bytes, two cache
lines have to be consumed with a total size of 128 bytes. It will land into the cache
but if no more data from this memory region will be needed, it will be waste of time.

[example](../src/sequential-memory-access/MemoryAccessBenchmarks.cs)

### Data Alignment

There is yet one other very important aspect of accessing memory. Most CPU
architectures are designed to access data that are properly aligned - meaning the starting
address of such data is a multiplication of a given alignment specified in bytes. Each type
has its own alignment and a data structure alignment depends on its field’s alignment.
A lot of care must be taken to not access unaligned data that may be a few times slower
than a proper way. This is a responsibility of the compiler and a developer designing
data structures. In case of CLR data structures, layout is mostly managed by the runtime
itself. This is why we can spot a lot of code related to proper alignment handling in the
Garbage Collector code.

### Hierarchical Cache

Returning to our architecture, due to performance requirements on the one hand and
cost optimization on the other, the CPU design evolved today into a more complex
hierarchical cache. The idea is simple. Instead of a single cache, let’s create a few, with
several different sizes and speeds. This allows you to create a very small and very fast
first-level cache (called L1), then a bit bigger and a bit slower cache level 2 (L2), and
finally the third-level cache (L3). This enumeration in modern architecture ends on three
levels.

The first-level cache is divided into two separate blocks. One is for data (labeled L1d)
and the other one for instructions (labeled L1i). The instructions read from the memory
and executed by the processor are also in fact data but interpreted appropriately. Data
and code instructions at levels higher than L1 are actually treated identically

Knowing that there are three main levels of cache, an obvious question arises - What
are the typical differences in speed and size between them and the main memory?
Memory at lower-cache levels can be fast enough that L1 and even L2 access may take up
enough CPU cycles to be faster than the pipeline execution time (unless you have to wait
for the exact address to be computed, which is also an expensive operation). So what do
those timings look like?

[cacheHittingPerformance](../src/sequential-read/SequentialReadBenchmark.cs)

### Multicore Hierarchical Cache

design. Contemporary
CPUs have a majority of more than one core. In simplified terms, the core is what the
individual, simplified processor is - it can execute code independently of other cores.
In the past, each core performed exactly one thread. Thus, a quad-core processor
could execute four threads simultaneously. At present, practically all processors have a
simultaneous multithreading mechanism (SMT), allowing simultaneous execution of two
threads within a single core. It is called Hyper-threading in case of Intel processors and
full SMT support has been added into AMD Zen microarchitecture.
As we can see, each of the cores has its own first- and second-level cache. The
third-level cache is shared between them. How cores and L3 cache are interconnected
is in fact an implementation detail.

But again, is this knowledge useful in such high-level environments as .NET? Does
Garbage Collector with its all knowledge and internal mechanisms hide such deep
hardware implementation details? The answer to this question can be found in the
following [example](../src/false-sharing-between-threads/ThreadBenchmark.cs).
Listing 2-6 shows multithreaded code that can simultaneously run a threadsCount
number of threads accessing the same sharedData array. Each of the thread just
increments a single element array without (theoretically) influencing other threads. In
our example, there are two important parameters indicating how those elements are
laid out within a shared array - whether there is a starting gap and how distant they are
from each other (offset). As we will run this code for threadsCount=4 on a four-core
machine, most probably each thread will have its own physical core assigned.
In Table 2-3 you can see significant differences in performance between various
combinations of gap and offset. If we use an array in definitely the most intuitive and
simple way, it means the gap is 0 and offset is 1. The layout and thread accesses are
illustrated in Figure 2-13a. This unfortunately introduces a very big cache-coherency
overhead.
Each thread (core) has its own local copy of the same memory region (in its
own cache line), so after each incrementation it has to invalidate the others’ local copies.
This forces cores to constantly invalidate their caches.
The obvious solution for this problem is to spread elements accessed by each thread
to different cache lines. The simplest way is to create a much bigger array and use only
every 16th element (16 times 4 bytes of single Int32 makes 64 bytes). This is a version
when offset is 16 and gap is still 0
There is still a single cache line constantly invalidated but it can be not so obvious
at the first glance, leading to a problem referred to as False sharing - an unfortunate
data access pattern in which theoretically not modified shared data is located within a
cache line altered by some other thread, incurring its constant invalidation.
In case of arrays there is important
data at the beginning of the object - the length of the array. What’s more, when accessing
array elements by an index operator, it internally checks whether it is not out of index.
This means accessing the beginning of the array object to check the length of the array,
every time we access any array element. Therefore, the first core is sharing the beginning
of the object with other cores, constantly invalidating correspondent cache lines.

[![GitHub last commit (master)](https://img.shields.io/github/last-commit/NetFabric/NetFabric.CodeAnalysis/master.svg?style=flat-square&logo=github)](https://github.com/NetFabric/NetFabric.CodeAnalysis/commits/master)
[![Build (master)](https://img.shields.io/github/workflow/status/NetFabric/NetFabric.CodeAnalysis/.NET%20Core/master.svg?style=flat-square&logo=github)](https://github.com/NetFabric/NetFabric.CodeAnalysis/actions)
[![Coverage](https://img.shields.io/coveralls/github/NetFabric/NetFabric.CodeAnalysis/master?style=flat-square&logo=coveralls)](https://coveralls.io/github/NetFabric/NetFabric.CodeAnalysis)
[![Gitter](https://img.shields.io/gitter/room/netfabric/netfabric.hyperlinq?style=flat-square&logo=gitter)](https://gitter.im/NetFabric/NetFabric.CodeAnalysis)

[![CodeAnalysis NuGet Version](https://img.shields.io/nuget/v/NetFabric.CodeAnalysis.svg?style=flat-square&label=CodeAnalysis%20nuget&logo=nuget)](https://www.nuget.org/packages/NetFabric.CodeAnalysis/)
[![CodeAnalysis NuGet Downloads](https://img.shields.io/nuget/dt/NetFabric.CodeAnalysis?style=flat-square&label=CodeAnalysis%20downloads&logo=nuget)](https://www.nuget.org/packages/NetFabric.CodeAnalysis/)

[![Reflection NuGet Version](https://img.shields.io/nuget/v/NetFabric.Reflection.svg?style=flat-square&label=Reflection%20nuget&logo=nuget)](https://www.nuget.org/packages/NetFabric.Reflection/)
[![Reflection NuGet Downloads](https://img.shields.io/nuget/dt/NetFabric.Reflection.svg?style=flat-square&label=Reflection%20downloads&logo=nuget)](https://www.nuget.org/packages/NetFabric.Reflection/)

# NetFabric.CodeAnalysis and NetFabric.Reflection

To find if a type is enumerable, it's not enough to check if it implements `IEnumerable`, `IEnumerable<>`, or `IAsyncEnumerable<>`. `foreach` and `await foreach` support several other cases as described [below](#sync-and-async-enumerables). This repository contains extension methods that take into account all these cases.

This repository is deployed as two NuGet packages:
- [NetFabric.CodeAnalysis](https://www.nuget.org/packages/NetFabric.CodeAnalysis/) - it can be used when parsing C# code, for example, in the development of for [Roslyn Analyzers](https://docs.microsoft.com/en-us/visualstudio/extensibility/getting-started-with-roslyn-analyzers) or [C# Code Generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/). Its used by the analyzer package [NetFabric.Hyperlinq.Analyzer](https://github.com/NetFabric/NetFabric.Hyperlinq.Analyzer) to implement rules for enumerables.
- [NetFabric.Reflection](https://www.nuget.org/packages/NetFabric.Reflection/) - it can be used in the runtime, for example, to optimize performance by using [Expression Trees](https://tyrrrz.me/blog/expression-trees). Its used by the package [NetFabric.Assertive](https://github.com/NetFabric/NetFabric.Assertive) to unit test any type of enumerable.

# Usage

## IsEnumerable() and IsEnumerator()

- Add either [NetFabric.CodeAnalysis](https://www.nuget.org/packages/NetFabric.CodeAnalysis/) or [NetFabric.Reflection](https://www.nuget.org/packages/NetFabric.Reflection/) packages to your project.
- Use the `IsEnumerable` or `IsEnumerator` methods as follow:
``` csharp
using NetFabric.CodeAnalysis;

var isEnumerable = typeSymbol.IsEnumerable(compilation, out var enumerableSymbols);
var isEnumerator = typeSymbol.IsEnumerator(compilation, out var enumeratorSymbols);

var isAsyncEnumerable = typeSymbol.IsAsyncEnumerable(compilation, out var asyncEnumerableSymbols);
var isAsyncEnumerator = typeSymbol.IsAsyncEnumerator(compilation, out var asyncEnumeratorSymbols);
```

``` csharp
using NetFabric.Reflection;

var isEnumerable = type.IsEnumerable(out var enumerableInfo);
var isEnumerator = type.IsEnumerator(out var enumeratorInfo);

var isAsyncEnumerable = type.IsAsyncEnumerable(out var asyncEnumerableInfo);
var isAsyncEnumerator = type.IsAsyncEnumerator(out var asyncEnumeratorInfo);
```

The methods return a boolean value indicating if it's a valid enumerable or enumerator. If `true`, the output parameter contains [`MethodInfo`](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodinfo) or [`IMethodSymbol`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.imethodsymbol) for the methods `GetEnumerator`, `get_Current` and `MoveNext`, following the precedences used by Roslyn for the `foreach` and `await foreach` keywords. It may also contain for methods `Reset` and `Dispose` if defined. Otherwise, `null`. They return the equivalent methods for async enumerables and enumerators.

## ExpressionEx

Add the [NetFabric.Reflection](https://www.nuget.org/packages/NetFabric.Reflection/) package to your project.

### ExpressionEx.ForEach

Add `ExpressionEx.ForEach` to an [Expression Trees](https://tyrrrz.me/blog/expression-trees) where:
- The first parameter defines an enumerable.
- The second parameter is the body defined by a `Func<Expression, Expression>`. You can pass a lambda expression that, given an Expression that defines an item, returns an Expression that uses it.

**WARNING:** Async enumerables are not supported.

The generated Expression depends on:

- The returned enumerator is an `interface`, `class`, `struct`, or `ref struct`.
- Is disposable or not.
- If it's an array, uses its indexer instead of `IEnumerable<>`.

Throws an exception if the Expression in the first parameter does not define an enumerable. In case

Here's an example that calculates the sum of the items in an enumerable:

``` csharp
using static NetFabric.Expressions.ExpressionEx;
using static System.Linq.Expressions.Expression;

int Sum<TEnumerable>(TEnumerable enumerable)
{
    var enumerableParameter = Parameter(typeof(TEnumerable), "enumerable");
    var sumVariable = Variable(typeof(int), "sum");
    var expression = Block(
        new[] {sumVariable},
        Assign(sumVariable, Constant(0)),
        ForEach(
            enumerableParameter,
            item => AddAssign(sumVariable, item)),
        sumVariable);
    var sum = Lambda<Func<TEnumerable, int>>(expression, enumerableParameter).Compile();

    return sum(enumerable);
}
```


### ExpressionEx.For

Add `ExpressionEx.For` to an [Expression Trees](https://tyrrrz.me/blog/expression-trees) where:

- The first parameter defines the initialization.
- The second parameter defines the condition.
- The third parameter defines the iterator.
- The fourth parameter defines the body.

Here's an example that calculates the sum of the items in an array:

``` csharp
using static NetFabric.Expressions.ExpressionEx;
using static System.Linq.Expressions.Expression;

int Sum(int[] array, int start, int end)
{
    var arrayParameter = Parameter(typeof(int[]), "array");
    var startParameter = Parameter(typeof(int), "start");
    var endParameter = Parameter(typeof(int), "end");
    var indexVariable = Variable(typeof(int), "index");
    var sumVariable = Variable(typeof(int), "sum");
    var expression = Block(
        new[] { indexVariable, sumVariable },
        Assign(sumVariable, Constant(0)),
        For(
            Assign(indexVariable, startParameter), 
            LessThan(indexVariable, endParameter), 
            PostIncrementAssign(indexVariable),
            AddAssign(sumVariable, ArrayIndex(arrayParameter, indexVariable))),
        sumVariable);
    var sum = Lambda<Func<int[], int, int, int>>(expression, arrayParameter, startParameter, endParameter).Compile();

    return sum(array, start, end);
}
```

### ExpressionEx.While

Add `ExpressionEx.While` to an [Expression Trees](https://tyrrrz.me/blog/expression-trees) where:

- The first parameter defines condition.
- The second parameter defines the body.

Here's an example that calculates the sum of the items in an array:

``` csharp
using static NetFabric.Expressions.ExpressionEx;
using static System.Linq.Expressions.Expression;

int Sum(int[] array, int start, int end)
{
    var valueParameter = Parameter(typeof(int[]), "value");
    var startParameter = Parameter(typeof(int), "start");
    var endParameter = Parameter(typeof(int), "end");
    var sumVariable = Variable(typeof(int), "sum");
    var indexVariable = Variable(typeof(int), "index");
    var expression = Block(
        new[] { indexVariable, sumVariable },
        Assign(sumVariable, Constant(0)),
        Assign(indexVariable, startParameter),
        While(
            LessThan(indexVariable, endParameter), 
            Block(
                AddAssign(sumVariable, ArrayIndex(valueParameter, indexVariable)),
                PostIncrementAssign(indexVariable)
            )
        ),
        sumVariable);
    var sum = Lambda<Func<int[], int, int, int>>(expression, valueParameter, startParameter, endParameter).Compile();

    return sum(array, start, end);
}
```

### ExpressionEx.Using

Add `ExpressionEx.Using` to an [Expression Trees](https://tyrrrz.me/blog/expression-trees) where:

- The first parameter defines the variable to be disposed.
- The second parameter defines the body.

Throws and exception if the variable is not disposable. To be considered disposable, if it's is a `class` or a `struct`, it has to implement the [`IDisposable`](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable) interface. If it's a `ref struct`, it only needs to have a parameterless `Dispose` method that returns `void`.

**WARNING:** `IAsyncDisposable` is not supported.

Here's an example that calculates the sum of the items in an enumerable:

``` csharp
int Sum<TEnumerable>(TEnumerable enumerable)
{
    if (!typeof(TEnumerable).IsEnumerable(out var enumerableInfo))
        throw new Exception("Not an enumerable!");
    
    var enumerableParameter = Parameter(typeof(TEnumerable), "enumerable");
    var enumeratorVariable = Variable(enumerableInfo.GetEnumerator.ReturnType, "enumerator");
    var sumVariable = Variable(typeof(int), "sum");
    var expression = Block(
        new[] {enumeratorVariable, sumVariable},
        Assign(enumeratorVariable, Call(enumerableParameter, enumerableInfo.GetEnumerator)),
        Assign(sumVariable, Constant(0)),
        Using(
            enumeratorVariable,
            While(
                Call(enumeratorVariable, enumerableInfo.EnumeratorInfo.MoveNext),
                AddAssign(sumVariable, Call(enumeratorVariable, enumerableInfo.EnumeratorInfo.GetCurrent))
            )
        ),
        sumVariable);
    var sum = Lambda<Func<TEnumerable, int>>(expression, enumerableParameter).Compile();
    
    return sum(enumerable);
}
```

# Sync and Async Enumerables

The code implemented in this repository can handle any of the following enumerable implementations:

### No enumerable interfaces

A collection, to be enumerated by a `foreach` loop, does not have to implement any interface. It just needs to have a parameterless `GetEnumerator()` method that returns a type that has a property `Current` with a getter and a parameterless `MoveNext()` method that returns `bool`.

The same applies to [async streams](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/generate-consume-asynchronous-stream) that, to be enumerated by an `await foreach` loop, doesn't need to implement any interface. It just needs to have a `GetAsyncEnumerator()` method, with no parameters or with a `CancellationToken`parameter, that returns a type that has a property `Current` with a getter and a parameterless `MoveNextAsync()` method that returns `ValueTask<bool>`.

Here's the minimal implementations for both types of enumerables:

``` csharp
public class Enumerable<T>
{
    public Enumerable<T> GetEnumerator() 
        => this;

    public T Current 
        => default;

    public bool MoveNext() 
        => false;
}
```

``` csharp
public class AsyncEnumerable<T>
{
    public AsyncEnumerable<T> GetAsyncEnumerator() 
        => this;

    public T Current 
        => default;

    public ValueTask<bool> MoveNextAsync() 
        => new ValueTask<bool>(false);
}
```

Here's the implementation of a range collection, for sync and async enumerables, returning values from 0 to `count`:

``` csharp
public readonly struct RangeEnumerable
{
    readonly int count;
    
    public RangeEnumerable(int count)
    {
        this.count = count;
    }
    
    public readonly Enumerator GetEnumerator() 
        => new Enumerator(count);
    
    public struct Enumerator
    {
        readonly int count;
        int current;
        
        internal Enumerator(int count)
        {
            this.count = count;
            current = -1;
        }
        
        public readonly int Current => current;
        
        public bool MoveNext() => ++current < count;
    }
}
```

``` csharp
public readonly struct RangeAsyncEnumerable
{
    readonly int count;

    public RangeAsyncEnumerable(int count)
    {
        this.count = count;
    }

    public Enumerator GetAsyncEnumerator(CancellationToken token = default)
        => new Enumerator(count, token);

    public struct Enumerator 
    {
        readonly int count;
        readonly CancellationToken token;
        int current;

        internal Enumerator(int count, CancellationToken token)
        {
            this.count = count;
            this.token = token;
            current = -1;
        }

        public readonly int Current => current;

        public ValueTask<bool> MoveNextAsync()
        {
            token.ThrowIfCancellationRequested();

            return new ValueTask<bool>(++current < count);
        }
    }
}
```

_NOTE: The advantage here is performance. The enumerator is a value type so the method calls are not virtual. The use of interfaces would box the enumerator and turn method calls into virtual. The enumerator is not disposable, so the `foreach` does not generate a `try`/`finally` clause, making it inlinable. The example also uses the C# 8 'struct read-only members' feature to avoid defensive copies._

_NOTE: In this case, `GetAsyncEnumerator()` has a `CancellationToken` parameter, which is also supported._

### Enumerable interfaces

Collections can implement `IEnumerable` and/or `IAsyncEnumerable<>`, or any interface derived from these, but the public `GetEnumerator()` and `GetAsyncEnumerator()` will take precedence.

``` csharp
public readonly struct MyRange : IReadOnlyCollection<int>
{    
    public MyRange(int count)
    {
        Count = count;
    }
    
    public readonly int Count { get; }
    
    public readonly Enumerator GetEnumerator() => new Enumerator(Count);
    readonly IEnumerator<int> IEnumerable<int>.GetEnumerator() => new DisposableEnumerator(Count);
    readonly IEnumerator IEnumerable.GetEnumerator() => new DisposableEnumerator(Count);
    
    public struct Enumerator
    {
        readonly int count;
        int current;
        
        internal Enumerator(int count)
        {
            this.count = count;
            current = -1;
        }
        
        public readonly int Current => current;
        
        public bool MoveNext() => ++current < count;
    }
    
    class DisposableEnumerator : IEnumerator<int>
    {
        readonly int count;
        int current;
        
        public DisposableEnumerator(int count)
        {
            this.count = count;
            current = -1;
        }
        
        public int Current => current;
        object IEnumerator.Current => current;
        
        public bool MoveNext() => ++current < count;
        
        public void Reset() => current = -1;
        
        public void Dispose() {}
    }
}
```

A `foreach` loop will use the more efficient public `GetEnumerator()` and the value-type non-disposable `Enumerator`. But, when casted to `IEnumerable<>`, the `IEnumerable<>.GetEnumerator()` and the reference-type `DisposableEnumerator` will be used.

An enumerable can contain only explicit interface implementations but these have to derive from `IEnumerable`:

``` csharp
public class MyRange : IEnumerable<int>
{    
    readonly int count;
    
    public MyRange(int count)
    {
        this.count = count;
    }

    IEnumerator<int> IEnumerable<int>.GetEnumerator() => new Enumerator(count);
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(count);
    
    class Enumerator : IEnumerator<int>
    {
        readonly int count;
        int current;
        
        internal Enumerator(int count)
        {
            this.count = count;
            current = -1;
        }
        
        int IEnumerator<int>.Current => current;
        object IEnumerator.Current => current;
        
        bool IEnumerator.MoveNext() => ++current < count;
        
        void IEnumerator.Reset() => current = -1;
        
        void IDisposable.Dispose() {}
    }
}
```

Using the following enumerable on a `foreach` will result in the error: `error CS1579: foreach statement cannot operate on variables of type 'MyRange' because 'MyRange' does not contain a public instance definition for 'GetEnumerator'`.

``` csharp
public interface MyIEnumerable<out T> 
{
    IEnumerator<T> GetEnumerator();
}

public class MyRange : MyIEnumerable<int>
{    
    readonly int count;
    
    public MyRange(int count)
    {
        this.count = count;
    }

    IEnumerator<int> MyIEnumerable<int>.GetEnumerator() => new Enumerator(count);
    
    class Enumerator : IEnumerator<int>
    {
        readonly int count;
        int current;
        
        internal Enumerator(int count)
        {
            this.count = count;
            current = -1;
        }
        
        int IEnumerator<int>.Current => current;
        object IEnumerator.Current => current;
        
        bool IEnumerator.MoveNext() => ++current < count;
        
        void IEnumerator.Reset() => current = -1;
        
        void IDisposable.Dispose() {}
    }
}
```

### Disposable enumerator

The enumerator can be disposable and it's disposed at the end of the enumeration.

If the enumerator is a `class` or a `struct`, it has to implement the [`IDisposable`](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable) interface. If it's a `ref struct`, it only needs to have a parameterless `Dispose` method that returns `void`.

### By-reference item return

The `Current` property can return the item by reference.

_NOTE: In this case, you should also be careful to declare the enumeration variable as `foreach (ref var item in source)` or `foreach (ref readonly var item in source)`. If you use `foreach (var item in source)`, no warning is shown and a copy of the item is made on each iteraton. You can use [NetFabric.CodeAnalysis.Analyzer](https://www.nuget.org/packages/NetFabric.CodeAnalysis.Analyzer/) to warn you of this case._

Here's a possible implementation of a sync enumerable:

``` csharp
public readonly struct WhereEnumerable<T>
{
    readonly T[] source;
    readonly Func<T, bool> predicate;
    
    public WhereEnumerable(T[] source, Func<T, bool> predicate)
    {
        this.source = source;
        this.predicate = predicate;
    }
    
    public Enumerator GetEnumerator() => new Enumerator(this);
    
    public struct Enumerator
    {
        readonly T[] source;
        readonly Func<T, bool> predicate;
        int index;
        
        internal Enumerator(in WhereEnumerable<T> enumerable)
        {
            source = enumerable.source;
            predicate = enumerable.predicate;
            index = -1;
        }
        
        public readonly ref readonly T Current => ref source[index];
        
        public bool MoveNext()
        {
            while (++index < source.Length)
            {
                if (predicate(source[index]))
                    return true;
            }
            return false;
        }

    }
}
```

## References

- [Enumeration in .NET](https://blog.usejournal.com/enumeration-in-net-d5674921512e) by Antão Almada

## Credits

The following open-source projects are used to build and test this project:

- [.NET](https://github.com/dotnet)
- [coveralls](https://coveralls.io)
- [coverlet](https://github.com/tonerdo/coverlet)
- [xUnit.net](https://xunit.net/)

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.


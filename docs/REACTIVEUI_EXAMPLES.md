# ReactiveUI Examples

This document shows how to use **DeepComparer** with [ReactiveUI](https://www.reactiveui.net/) collections.

---

## Supported Collections
- `SourceList<T>`
- `SourceCache<TKey, TValue>`

Comparison works by:
1. Comparing **collection size** first.
2. Recursively comparing elements (order-insensitive).

---

## Example 1 – Comparing `SourceList<T>`

```csharp
using DynamicData;
using DeepComparerNS;
using Xunit;

public class ReactiveUITests
{
    [Fact]
    public void Compare_SourceList_Equal()
    {
        var list1 = new SourceList<int>();
        var list2 = new SourceList<int>();

        list1.AddRange(new[] { 1, 2, 3 });
        list2.AddRange(new[] { 1, 3, 2 });

        var result = DeepComparer.Compare(list1, list2);

        Assert.True(result);
    }
}
```
---
Note: Order does not affect equality.

## Example 2 – Comparing SourceCache<TKey, TValue>
```csharp  
using DynamicData;
using DeepComparerNS;
using Xunit;

public class ReactiveUITests
{
    [Fact]
    public void Compare_SourceCache_Equal()
    {
        var cache1 = new SourceCache<Item, int>(x => x.Id);
        var cache2 = new SourceCache<Item, int>(x => x.Id);

        cache1.AddOrUpdate(new[] { new Item(1, "A"), new Item(2, "B") });
        cache2.AddOrUpdate(new[] { new Item(2, "B"), new Item(1, "A") });

        var result = DeepComparer.Compare(cache1, cache2);

        Assert.True(result);
    }

    public record Item(int Id, string Name);
}
```
---

## Example 3 – Custom Comparison with CompareOptions

```csharp
var options = new DeepComparer.CompareOptions
{
    CustomSimpleTypePredicate = t => t == typeof(SourceList<int>)
};

var result = DeepComparer.CompareWithReport(list1, list2, true, true, true, options);

if (!result.AreEqual)
{
    foreach (var diff in result.Differences)
        Console.WriteLine(diff);
}
```

## Example 3 – Custom Comparison with CompareOptions

```csharp
var options = new CompareOptions
{
    CustomSimpleTypePredicate = t => t == typeof(SourceList<int>)
};

var result = DeepComparer.CompareWithReport(list1, list2, true, true, true, options);

if (!result.AreEqual)
{
    foreach (var diff in result.Differences)
        Console.WriteLine(diff);
}
```
---

## Performance Note

- DeepComparer extracts items from SourceList or SourceCache via public API.

- Collection size is checked first to avoid unnecessary item-level comparisons.

---

## Best Practices

- Use CustomSimpleTypePredicate for custom ReactiveUI-derived classes.

- Avoid comparing very large SourceCache or SourceList inside UI loops.

- Cache results if comparing frequently updated collections.



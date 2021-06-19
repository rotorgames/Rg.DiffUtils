![Header](img/header.png)
# Rg.DiffUtils ![Nuget](https://img.shields.io/nuget/v/Rg.DiffUtils)

Rg.DiffUtils is a powerful tool that allows to merge collections and find differences.

The library uses Eugene W. Myers's algorithm to find differences and runs the second pass to find movements.

The difference between this library and the rest that this library expands the `ObservableCollection` and allows to notify UI or other elements about all changes in collections. 

## How To Use

### DiffObservableCollection

In most cases, you can use the `DiffObservableCollection` for merging collections.

The class provides the `ReplaceDiff` method to merge a new collection into existing.

```csharp
var array1 = new int[] { 1, 2, 3, 4, 5 };
var array2 = new int[] { 4, 5, 1, 6, 7 };

var collection = new DiffObservableCollection<int>(array1);

collection.ReplaceDiff(array2);
```

To transform `array1` to `array2` the library runs 3 steps:
- Add `6` and `7` to 5 and 6 positions
- Remove `2` and `3` from 1 and 2 positions
- Move `1` from 0 to 2 position

As you can see the library tries to optimize manipulations and batch several operations to one step. You can change this behavior in [DiffOptions](#diffoptions)

### DiffUtil
However, there are times when you can't use the `DiffObservableCollection`. For example, when you use your own implementation of the `IEnumerable`. 

In this case, you can use the `DiffUtil` to calculate all steps for merging collections.

```csharp
var array1 = new YourOwnCollection();
var array2 = new int[] { 4, 5, 1, 6, 7 };

var result = DiffUtil.CalculateDiff(array1, array2);
```

`DiffUtil.CalculateDiff` returns [DiffResult](#diffresult). You can use the `Steps` property to mutate your own collection step by step.

### DiffOptions
`DiffUtil.CalculateDiff`, `DiffObservableCollection.ReplaceDiff` and the `DiffObservableCollection` constructor can take an optional class the `DiffOptions`

```csharp
var options = new DiffOptions
{
    AllowBatching = true, //Default: true. If it's false, all steps contain only one item even if items follow each other
    DetectMoves = true //Default: true. If it's false, the second pass for movements detection doesn't work
};
```

### IDiffEqualityComparer
By default `DiffUtil.CalculateDiff` uses `EqualityComparer<T>.Default` for comparing items but you can implement your own comparator.

For this case create `DiffEqualityComparer` which takes `IEqualityComparer` or a delegate in the constructor.

```csharp
var diffComparer = new DiffEqualityComparer<int>(new YourEqualityComparer());
// or
var diffComparer = new DiffEqualityComparer<int>((int oldItem, int newItem) => oldItem == newItem);
```
or implement `IDiffEqualityComparer`

```csharp
class MyDiffEqualityComparer : IDiffEqualityComparer<int>
{
    public bool CompareItems(int oldItem, int newItem)
    {
        return oldItem == newItem;
    }
}
```

`IDiffEqualityComparer` can be used in `DiffUtil.CalculateDiff` but in `DiffObservableCollection` you have to use [IDiffHandler](#idiffhandler)

Also, you can use `ToDiffEqualityComparer` extension method to convert `IEqualityComparer` to `DiffEqualityComparer`

### IDiffHandler
`IDiffHandler` inherits from `IDiffEqualityComparer` and contains a new method `UpdateItem`.

The difference between `IDiffHandler` and `IDiffEqualityComparer` that the `IDiffHandler` updates equaled elements that allows applying new data to an existing item.

```csharp
class MyDiffHandler : IDiffHandler<MyCustomClass>
{
    public bool CompareItems(MyCustomClass oldItem, MyCustomClass newItem)
    {
        return oldItem.Id == newItem.Id;
    }

    public void UpdateItem(MyCustomClass oldItem, MyCustomClass newItem)
    {
        // this method is invoked for each item if CompareItems was true

        if (oldItem.Name != newItem.Name)
            oldItem.Name = newItem.Name;
    }
}
````

If you don't want to implement `IDiffHandler` you can use `DiffHandler` which takes `IDiffEqualityComparer`, `IEqualityComparer`, or a delegate as the first parameter, and an optional delegate which updates items as the second parameter

```csharp
var array = new int[] { 4, 5, 1, 6, 7 };
var collection = new DiffObservableCollection<int>();

collection.ReplaceDiff(array, new DiffHandler<int>(new YourDiffEqualityComparer(), UpdateItems /*optional*/);
//or
collection.ReplaceDiff(array, new DiffHandler<int>(new YourEqualityComparer(), UpdateItems /*optional*/);
//or
collection.ReplaceDiff(array, new DiffHandler<int>((int x, int y) => x == y, UpdateItems /*optional*/);

void UpdateItems(int oldItem, int newItem)
{
    // update oldItem here
}
```

You can use the `ToDiffHandler` extension method to convert `IEqualityComparer` or `IDiffEqualityComparer` to `DiffHandler`

### DiffResult
`DiffResult` contains all information about merging the first collection to the second.

- `T[] OldSequence` The old collection converted to Array
- `T[] NewSequence` The new collection converted to Array
- `IReadOnlyList<DiffItem<T>> SameItems` MovedItems and NotMovedItems items
- `IReadOnlyList<DiffItem<T>> MovedItems` All moved same items
- `IReadOnlyList<DiffItem<T>> NotMovedItems` All not moved same items
- `IReadOnlyList<DiffItem<T>> RemovedItems` All items which were removed
- `IReadOnlyList<DiffItem<T>> AddedItems` All items which were added
- `IReadOnlyList<DiffStep<T>> Steps` All the necessary steps to merge the new collection to the old

## Thanks
https://github.com/androidx/androidx

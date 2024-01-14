using IconMan.Util;

namespace IconMan.Tests;

public class MoreLinqTest
{
    [Fact]
    public void RemoveFirstRangeWhere_RemovesInnerRange()
    {
        var list = new List<int> { 1, 2, 2, 2, 3 };
        int removed = list.RemoveFirstRangeWhere(x => x == 2);
        Assert.Equal(3, removed);
        Assert.Equal([1, 3], list);
    }

    [Fact]
    public void RemoveFirstRangeWhere_RemovesInnerRange_Only()
    {
        var list = new List<int> { 1, 2, 2, 2, 3, 2 };
        int removed = list.RemoveFirstRangeWhere(x => x == 2);
        Assert.Equal(3, removed);
        Assert.Equal([1, 3, 2], list);
    }

    [Fact]
    public void RemoveFirstRangeWhere_RemovesStartRange()
    {
        var list = new List<int> { 1, 1, 2, 3 };
        int removed = list.RemoveFirstRangeWhere(x => x == 1);
        Assert.Equal(2, removed);
        Assert.Equal([2, 3], list);
    }

    [Fact]
    public void RemoveFirstRangeWhere_RemovesEndRange()
    {
        var list = new List<int> { 1, 2, 3, 3, 3 };
        int removed = list.RemoveFirstRangeWhere(x => x == 3);
        Assert.Equal(3, removed);
        Assert.Equal([1, 2], list);
    }

    [Fact]
    public void RemoveFirstRangeWhere_RemovesNoting()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        int removed = list.RemoveFirstRangeWhere(x => x == 0);
        Assert.Equal(0, removed);
        Assert.Equal([1, 2, 3, 4, 5], list);
    }

    [Fact]
    public void RemoveFirstRangeWhere_RemovesFirstOne()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        int removed = list.RemoveFirstRangeWhere(x => x == 1);
        Assert.Equal(1, removed);
        Assert.Equal([2, 3, 4, 5], list);
    }

    [Fact]
    public void RemoveFirstRangeWhere_RemovesLastOne()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        int removed = list.RemoveFirstRangeWhere(x => x == 5);
        Assert.Equal(1, removed);
        Assert.Equal([1, 2, 3, 4], list);
    }
}

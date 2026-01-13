using System;
using System.Collections.Generic;
using System.Text;

namespace PokerDatabaseCli2._0.Common;

public static class Collections {
    public static bool 
    TryGet<T>(this IEnumerable<T> items, Func<T, bool> predicate, out T found) {
        foreach(var item in items) {
            if (predicate(item)) {
                found = item;
                return true;
            }
        }
        found = default!;
        return false;
    }

    public static IEnumerable<T> TakeWhileAccum<T>(this IEnumerable<T> source, Func<T, bool> predicate, int maxCount)
{
    int count = 0;
    foreach (var item in source)
    {
        if (!predicate(item) || count >= maxCount)
            break;
        yield return item;
        count++;
    }
}

}

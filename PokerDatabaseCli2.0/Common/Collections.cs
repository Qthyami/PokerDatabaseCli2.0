
namespace PokerDatabaseCli2._0.Common;

public static class Collections {
    public static bool
    TryGet<T>(this IEnumerable<T> items, Func<T, bool> predicate, out T found) {
        foreach (var item in items) {
            if (predicate(item)) {
                found = item;
                return true;
            }
        }
        found = default!;
        return false;
    }

}

using System.Collections.Generic;
using System.Linq;

namespace DLeh.Console
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> list) => list ?? Enumerable.Empty<T>();
    }
}

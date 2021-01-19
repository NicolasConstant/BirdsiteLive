using System.Collections.Generic;
using System.Linq;

namespace BirdsiteLive.Common.Extensions
{
    public static class EnumerableExt
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace TnT.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }

            return source;
        }

        public static IEnumerable<List<T>> GetPermutations<T>(this IEnumerable<T> list, int length)
        {
            if (length == 1)
                return list.Select(t => new List<T> { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                            (t1, t2) => new List<T>(t1) { t2 });
        }

        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            Random rand = new Random();
            return source.OrderBy(x => rand.Next());
            // return source.OrderBy(x => Guid.NewGuid());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, int seed)
        {
            Random rand = new Random(seed);
            return source.OrderBy(x => rand.Next());
        }


        public static IEnumerable<T> Rotate<T>(this IEnumerable<T> source, int k)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();
            int n = list.Count;

            if (n == 0 || k == 0)
                return list; // Nothing to rotate

            k %= n; // Reduce unnecessary full rotations

            if (k < 0)
                k += n; // Convert negative rotations to equivalent positive ones

            // Return rotated sequence without modifying the original
            return list.Skip(n - k).Concat(list.Take(n - k));
        }
    }
}
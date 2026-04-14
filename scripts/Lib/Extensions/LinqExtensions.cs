
using System;
using System.Collections.Generic;
using System.Linq;

namespace TnT.Extensions
{
    /// <summary>
    /// Extension methods that supplement the standard LINQ API with additional
    /// collection operations: iteration, permutations, randomisation, rotation, and padding.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Executes <paramref name="action"/> for every element in <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The sequence to iterate.</param>
        /// <param name="action">The delegate to invoke for each element.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }

        // public static IEnumerable<List<T>> GetPermutations<T>(this IEnumerable<T> list, int length)
        // {
        //     if (length == 1)
        //         return list.Select(t => new List<T> { t });

        //     return GetPermutations(list, length - 1)
        //         .SelectMany(t => list.Where(e => !t.Contains(e)),
        //                     (t1, t2) => new List<T>(t1) { t2 });
        // }

        /// <summary>
        /// Returns all permutations of <paramref name="length"/> distinct elements drawn from <paramref name="source"/>.
        /// Each permutation is yielded as a separate <see cref="IEnumerable{T}"/> snapshot,
        /// so elements are not reused within a single permutation.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The pool of elements to permute.</param>
        /// <param name="length">The number of elements in each permutation.</param>
        /// <returns>A lazy sequence of all permutations of the requested length.</returns>
        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> source, int length)
        {
            var list = source.ToList();
            var used = new bool[list.Count];
            var current = new T[length];

            return Permute(list, used, current, 0, length);
        }

        private static IEnumerable<IEnumerable<T>> Permute<T>(
            List<T> list, bool[] used, T[] current, int depth, int length)
        {
            if (depth == length)
            {
                yield return current.ToArray(); // snapshot
                yield break;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (used[i]) continue;
                used[i] = true;
                current[depth] = list[i];
                foreach (var perm in Permute(list, used, current, depth + 1, length))
                    yield return perm;
                used[i] = false;
            }
        }

        private static readonly Random _rand = new Random();

        /// <summary>
        /// Returns the elements of <paramref name="source"/> in a random order
        /// using a shared <see cref="Random"/> instance.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The sequence to shuffle.</param>
        /// <returns>The shuffled sequence.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
            => source.OrderBy(x => _rand.Next());

        /// <summary>
        /// Returns a single element chosen at random from <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The sequence to pick from.</param>
        /// <returns>A randomly selected element.</returns>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            var list = source as IList<T> ?? source.ToList();
            return list[_rand.Next(list.Count)];
        }

        /// <summary>
        /// Returns <paramref name="count"/> elements chosen at random from <paramref name="source"/>
        /// without replacement.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The sequence to pick from.</param>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>A sequence of <paramref name="count"/> randomly selected elements.</returns>
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        /// <summary>
        /// Returns the elements of <paramref name="source"/> in a random order
        /// using a seeded <see cref="Random"/> instance, producing a deterministic shuffle.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The sequence to shuffle.</param>
        /// <param name="seed">The seed for the random number generator.</param>
        /// <returns>The deterministically shuffled sequence.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, int seed)
        {
            Random rand = new Random(seed);
            return source.OrderBy(x => rand.Next());
        }


        /// <summary>
        /// Rotates <paramref name="source"/> by <paramref name="k"/> positions to the right.
        /// Negative values rotate to the left. Full-cycle rotations (multiples of the sequence
        /// length) are reduced automatically. The original sequence is not modified.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The sequence to rotate.</param>
        /// <param name="k">
        /// The number of positions to rotate. Positive values shift elements toward the end;
        /// negative values shift elements toward the beginning.
        /// </param>
        /// <returns>A new sequence with elements rotated by <paramref name="k"/> positions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Specifies how padding elements are distributed around the original sequence
        /// when using <see cref="Pad{T}"/>.
        /// </summary>
        public enum PadAlignment
        {
            /// <summary>Padding is added to the left (before) the sequence.</summary>
            Left,
            /// <summary>Padding is split evenly; when the total is odd, the right side receives the extra element.</summary>
            Center,
            /// <summary>Padding is added to the right (after) the sequence.</summary>
            Right
        }

        /// <summary>
        /// Expands <paramref name="source"/> to exactly <paramref name="newSize"/> elements
        /// by inserting <paramref name="paddingValue"/> elements on one or both sides
        /// according to <paramref name="alignment"/>.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The sequence to pad.</param>
        /// <param name="newSize">The desired total length. Must be &gt;= the source length.</param>
        /// <param name="alignment">
        /// Where the padding is placed. Defaults to <see cref="PadAlignment.Center"/>.
        /// </param>
        /// <param name="paddingValue">
        /// The value inserted as padding. Defaults to <c>default(T)</c>.
        /// </param>
        /// <returns>A new sequence of length <paramref name="newSize"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="newSize"/> is less than the source length.</exception>
        public static IEnumerable<T> Pad<T>(
            this IEnumerable<T> source,
            int newSize,
            PadAlignment alignment = PadAlignment.Center,
            T paddingValue = default!)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceList = source.ToList();
            int originalSize = sourceList.Count;

            if (newSize < originalSize)
                throw new ArgumentException("New size must be greater than or equal to the original size.");

            int totalPadding = newSize - originalSize;
            int rightPadding = 0;

            switch (alignment)
            {
                case PadAlignment.Right:
                    rightPadding = 0;
                    break;

                case PadAlignment.Left:
                    rightPadding = totalPadding;
                    break;

                case PadAlignment.Center:
                    // For even padding, right gets more
                    rightPadding = totalPadding / 2;
                    break;
            }

            int leftPadding = totalPadding - rightPadding;

            // Yield left padding
            for (int i = 0; i < leftPadding; i++)
                yield return paddingValue;

            // Yield original elements
            foreach (var item in sourceList)
                yield return item;

            // Yield right padding
            for (int i = 0; i < rightPadding; i++)
                yield return paddingValue;
        }
    }
}
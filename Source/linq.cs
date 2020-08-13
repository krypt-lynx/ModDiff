// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqExtansions
{

    #region ingame script start

    static class LinqExtansions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var t in enumerable)
                action(t);
        }

        /// <summary>
        /// Wraps this object instance into an IEnumerable&lt;T&gt;
        /// consisting of a single item.
        /// </summary>
        /// <typeparam name="T"> Type of the object. </typeparam>
        /// <param name="item"> The instance that will be wrapped. </param>
        /// <returns> An IEnumerable&lt;T&gt; consisting of a single item. </returns>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static T CastValue<T>(this object o) where T : class
        {
            return o as T;
        }

        public static int IndexOfMax<T>(this IEnumerable<T> enumerable, IComparer<T> comparer = null)
        {
            if (comparer == null) comparer = Comparer<T>.Default;

            var e = enumerable.GetEnumerator();
            if (!e.MoveNext())
            {
                return -1;
            }
            T max = e.Current;
            int maxI = 0;
            int i = 1;
            while (e.MoveNext())
            {
                if (comparer.Compare(e.Current, max) > 0)
                {
                    maxI = i;
                    max = e.Current;
                }

                i++;
            }

            return maxI;
        }

        /// <summary>
        /// Returns all elements of <paramref name="source"/> without <paramref name="elements"/>.
        /// Does not throw an exception if <paramref name="source"/> does not contain <paramref name="elements"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{TSource}"/> to remove the specified elements from.</param>
        /// <param name="elements">The elements to remove.</param>
        /// <returns>
        /// All elements of <paramref name="source"/> except <paramref name="elements"/>.
        /// </returns>
        public static IEnumerable<TSource> Without<TSource>(this IEnumerable<TSource> source, params TSource[] elements)
        {
            return Without(source, (IEnumerable<TSource>)elements);
        }

        /// <summary>
        /// Returns all elements of <paramref name="source"/> without <paramref name="elements"/>.
        /// Does not throw an exception if <paramref name="source"/> does not contain <paramref name="elements"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{TSource}"/> to remove the specified elements from.</param>
        /// <param name="elements">The elements to remove.</param>
        /// <returns>
        /// All elements of <paramref name="source"/> except <paramref name="elements"/>.
        /// </returns>
        public static IEnumerable<TSource> Without<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> elements)
        {
            //ThrowIf.Argument.IsNull(source, "source");
            //ThrowIf.Argument.IsNull(elements, "elements");

            return WithoutIterator(source, elements, EqualityComparer<TSource>.Default);
        }

        /// <summary>
        /// Returns all elements of <paramref name="source"/> without <paramref name="elements"/> using the specified equality comparer to compare values.
        /// Does not throw an exception if <paramref name="source"/> does not contain <paramref name="elements"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{TSource}"/> to remove the specified elements from.</param>
        /// <param name="equalityComparer">The equality comparer to use.</param>
        /// <param name="elements">The elements to remove.</param>
        /// <returns>
        /// All elements of <paramref name="source"/> except <paramref name="elements"/>.
        /// </returns>
        public static IEnumerable<TSource> Without<TSource>(this IEnumerable<TSource> source,
            IEqualityComparer<TSource> equalityComparer, params TSource[] elements)
        {
            return Without(source, equalityComparer, (IEnumerable<TSource>)elements);
        }

        /// <summary>
        /// Returns all elements of <paramref name="source"/> without <paramref name="elements"/> using the specified equality comparer to compare values.
        /// Does not throw an exception if <paramref name="source"/> does not contain <paramref name="elements"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{TSource}"/> to remove the specified elements from.</param>
        /// <param name="equalityComparer">The equality comparer to use.</param>
        /// <param name="elements">The elements to remove.</param>
        /// <returns>
        /// All elements of <paramref name="source"/> except <paramref name="elements"/>.
        /// </returns>
        public static IEnumerable<TSource> Without<TSource>(this IEnumerable<TSource> source,
            IEqualityComparer<TSource> equalityComparer, IEnumerable<TSource> elements)
        {
            //ThrowIf.Argument.IsNull(source, "source");
            // ThrowIf.Argument.IsNull(elements, "elements");
            // ThrowIf.Argument.IsNull(equalityComparer, "equalityComparer");

            return WithoutIterator(source, elements, equalityComparer);
        }

        private static IEnumerable<TSource> WithoutIterator<TSource>(IEnumerable<TSource> source,
            IEnumerable<TSource> elementsToRemove, IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> elementsToRemoveSet = new HashSet<TSource>(elementsToRemove, comparer);

            return source.Where(elem => !elementsToRemoveSet.Contains(elem));

        }

    }


	#endregion // ingame script end

}

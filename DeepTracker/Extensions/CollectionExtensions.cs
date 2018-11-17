using System;
using System.Collections.Generic;

namespace DeepTracker1.Extensions
{
    public static class CollectionExtensions
    {
        #region Static members

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> collection, TKey key, Func<TKey, TValue> addFactory = null)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (collection.ContainsKey(key)) return collection[key];
            addFactory = addFactory ?? (factoryKey => Activator.CreateInstance<TValue>());
            var result = addFactory(key);
            collection.Add(key, result);
            return result;
        }

        /// <summary>
        ///     Enumerates several IEnumerable instances as one.
        /// </summary>
        /// <param name="enumerable">Source object.</param>
        /// <param name="unitedItems">United item instances.</param>
        /// <returns>Enumerable union/</returns>
        public static IEnumerable<T> UnionWith<T>(this IEnumerable<T> enumerable, params T[] unitedItems)
        {
            foreach (var item in enumerable)
            {
                yield return item;
            }

            foreach (var item in unitedItems)
            {
                yield return item;
            }
        }

        #endregion
    }
}
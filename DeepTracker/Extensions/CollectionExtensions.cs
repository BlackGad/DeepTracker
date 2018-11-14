using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepTracker.Extensions
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

        public static T GetOrAdd<T>(this ICollection<T> collection, Func<T, bool> predicate, Func<T> addFactory = null)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var selected = collection.Where(predicate).ToList();
            if (selected.Any()) return selected.First();

            addFactory = addFactory ?? Activator.CreateInstance<T>;
            var item = addFactory();
            collection.Add(item);
            return item;
        }

        #endregion
    }
}
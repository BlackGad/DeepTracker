using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DeepTracker.Data
{
    public class ObjectsStorage<TKey, TObject> : IEnumerable<KeyValuePair<TKey, TObject>>
    {
        private readonly Func<TKey, TObject> _factory;
        private readonly ConcurrentDictionary<TKey, Lazy<TObject>> _storage;

        #region Constructors

        public ObjectsStorage(Func<TKey, TObject> factory = null)
        {
            _factory = factory ?? (key => Activator.CreateInstance<TObject>());
            _storage = new ConcurrentDictionary<TKey, Lazy<TObject>>();
        }

        #endregion

        #region Properties

        public TObject this[TKey key]
        {
            get { return GetOrCreate(key); }
        }

        public TKey[] Keys
        {
            get { return _storage.Keys.ToArray(); }
        }

        public TObject[] Values
        {
            get { return _storage.Values.Select(v => v.Value).ToArray(); }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TObject>> Members

        public IEnumerator<KeyValuePair<TKey, TObject>> GetEnumerator()
        {
            return _storage.Select(p => new KeyValuePair<TKey, TObject>(p.Key, p.Value.Value))
                           .ToList()
                           .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Members

        public TObject Attach(TKey key, TObject obj)
        {
            return _storage.GetOrAdd(key, k => new Lazy<TObject>(() => obj, true)).Value;
        }

        public TObject Create(TKey key, Func<TKey, TObject> factory = null)
        {
            factory = factory ?? _factory;
            return factory(key);
        }

        public TObject GetOrCreate(TKey key, Func<TKey, TObject> factory = null)
        {
            return _storage.GetOrAdd(key, k => new Lazy<TObject>(() => Create(k, factory), true)).Value;
        }

        public TObject Remove(TKey key)
        {
            if (_storage.TryRemove(key, out var obj)) return obj.Value;
            return default(TObject);
        }

        #endregion
    }
}
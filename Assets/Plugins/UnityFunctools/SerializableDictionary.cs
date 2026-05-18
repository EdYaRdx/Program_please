using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityFunctools
{
    [Serializable]
    public class SerializableDictionary<K, V>
    {
        [SerializeField] private Pair[] _pairs;

        private Dictionary<K, V> _collection;

        public Dictionary<K, V> Collection
        {
            get 
            {
                if (_collection == null)
                    _collection = ToDictionary(_pairs);

                return _collection;
            }
        }

        public void Save()
        {
            K[] keys = Collection.Keys.ToArray();

            _pairs = new Pair[keys.Length];

            for (int i = 0; i < keys.Length; i++)
            {
                K currentKey = keys[i];
                _pairs[i] = new Pair(currentKey, Collection[currentKey]);
            }
        }

        private Dictionary<K, V> ToDictionary(Pair[] pairs)
        {
            Dictionary<K, V> collection = new();

            if (pairs != null)
            {
                foreach (Pair pair in pairs)
                {
                    if (collection.ContainsKey(pair.Key))
                    {
                        throw new Exception();
                    }

                    collection.Add(pair.Key, pair.Value);
                }
            }

            return collection;
        }

        [Serializable]
        private class Pair
        {
            public K Key;
            public V Value;

            public Pair(K key, V value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
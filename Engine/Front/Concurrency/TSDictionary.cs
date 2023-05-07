using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Concurrency
{
    public class TSDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        public readonly object syncRoot = new object();
        private Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

        public TValue this[TKey key] { 
            get {
               // lock (syncRoot)
                    return dict[key]; 
            } 
            set {
                    dict[key] = value;
            } 
        }
        

        public ICollection<TKey> Keys => dict.Keys;

        public ICollection<TValue> Values => dict.Values;

        public Int32 Count => dict.Count;

        public Boolean IsReadOnly => throw new NotImplementedException();

       

        public void Add(TKey key, TValue value)
        {

            //Dictionarys indexer will add a key if it doesn't exits.
            dict[key] = value;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                dict.Add(item.Key, item.Value);
            }
        }

        public void Clear()
        {
            lock (syncRoot)
            {
                dict.Clear();
            }
        }

        public Boolean Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                return dict.Contains(item);
            }
        }
        public Boolean ContainsKey(TKey key)
        {
            //lock (syncRoot)
            //try
            {
                return dict.ContainsKey(key);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Boolean Remove(TKey key)
        {
            lock (syncRoot)
            {
                return dict.Remove(key);
            }
        }

        public Boolean Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (syncRoot)
            {
                return dict.Remove(item.Key);
            }
        }

        public Boolean TryGetValue(TKey key, out TValue value)
        {
            lock (syncRoot)
            {
                return dict.TryGetValue(key, out value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

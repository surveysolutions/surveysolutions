using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Mono.Android.Crasher.Data
{
    /// <summary>
    /// Stores a crash reports data with <see cref="ReportField"/> enum values as keys.
    /// </summary>
    public class ReportData : IDictionary<string, string>
    {
        private readonly ConcurrentDictionary<string, string> _innerDictionary;

        public ReportData()
        {
            _innerDictionary = new ConcurrentDictionary<string, string>();
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var item in this)
            {
                builder.AppendFormat("{0} = {1}", item.Key, item.Value).AppendLine();
            }
            return builder.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            if (!_innerDictionary.ContainsKey(item.Key))
                _innerDictionary.TryAdd(item.Key, item.Value);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return _innerDictionary.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((IDictionary<string, string>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            string value;
            return _innerDictionary.ContainsKey(item.Key) && _innerDictionary.TryRemove(item.Key, out value);
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(ReportField key, string value)
        {
            Add(key.ToString(), value);
        }

        public void Add(string key, string value)
        {
            if (_innerDictionary.ContainsKey(key))
                _innerDictionary[key] = value;
            else
                _innerDictionary.TryAdd(key, value);
        }

        public bool ContainsKey(ReportField key)
        {
            return ContainsKey(key.ToString());
        }

        public bool ContainsKey(string key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public bool Remove(ReportField key)
        {
            return Remove(key.ToString());
        }

        public bool Remove(string key)
        {
            string value;
            return _innerDictionary.ContainsKey(key) && _innerDictionary.TryRemove(key, out value);
        }

        public bool TryGetValue(ReportField key, out string value)
        {
            return TryGetValue(key.ToString(), out value);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        public string this[ReportField key]
        {
            get
            {
                return this[key.ToString()];
            }
            set
            {
                _innerDictionary[key.ToString()] = value;
            }
        }

        public string this[string key]
        {
            get
            {
                string obj;
                _innerDictionary.TryGetValue(key, out obj);
                return obj ?? string.Empty;
            }
            set
            {
                _innerDictionary[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public ICollection<string> Values
        {
            get { return _innerDictionary.Values; }
        }
    }
}
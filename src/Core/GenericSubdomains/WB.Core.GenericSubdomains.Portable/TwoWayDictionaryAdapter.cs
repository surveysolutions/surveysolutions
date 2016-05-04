using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public class TwoWayDictionaryAdapter<TKey, TAdaptee, TTarget> : IDictionary<TKey, TTarget>
    {
        private readonly Func<IDictionary<TKey, TAdaptee>> getAdapteeDictionary;
        private readonly Func<TAdaptee, TTarget> convertAdapteeToTarget; 
        private readonly Func<TTarget, TAdaptee> convertTargetToAdaptee; 

        public TwoWayDictionaryAdapter(
            Func<IDictionary<TKey, TAdaptee>> getAdapteeDictionary,
            Func<TAdaptee, TTarget> convertAdapteeToTarget,
            Func<TTarget, TAdaptee> convertTargetToAdaptee)
        {
            this.getAdapteeDictionary = getAdapteeDictionary;
            this.convertAdapteeToTarget = convertAdapteeToTarget;
            this.convertTargetToAdaptee = convertTargetToAdaptee;
        }

        private IDictionary<TKey, TAdaptee> AdapteeDictionary => this.getAdapteeDictionary();

        public IEnumerator<KeyValuePair<TKey, TTarget>> GetEnumerator()
            => this.AdapteeDictionary.Select(this.ConvertAdapteeToTarget).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable) this.AdapteeDictionary).GetEnumerator();

        public void Add(KeyValuePair<TKey, TTarget> item)
            => this.AdapteeDictionary.Add(this.ConvertTargetToAdaptee(item));

        public void Clear()
            => this.AdapteeDictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TTarget> item)
            => this.AdapteeDictionary.Contains(this.ConvertTargetToAdaptee(item));

        public void CopyTo(KeyValuePair<TKey, TTarget>[] array, int arrayIndex)
            => this.AdapteeDictionary.Select(this.ConvertAdapteeToTarget).ToList().CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<TKey, TTarget> item)
            => this.AdapteeDictionary.Remove(this.ConvertTargetToAdaptee(item));

        public int Count
            => this.AdapteeDictionary.Count;

        public bool IsReadOnly
            => this.AdapteeDictionary.IsReadOnly;

        public void Add(TKey key, TTarget value)
            => this.AdapteeDictionary.Add(key, this.convertTargetToAdaptee(value));

        public bool ContainsKey(TKey key)
            => this.AdapteeDictionary.ContainsKey(key);

        public bool Remove(TKey key)
            => this.AdapteeDictionary.Remove(key);

        public bool TryGetValue(TKey key, out TTarget value)
        {
            TAdaptee adapteeValue;
            bool succeeded = this.AdapteeDictionary.TryGetValue(key, out adapteeValue);

            value = succeeded ? this.convertAdapteeToTarget(adapteeValue) : default(TTarget);

            return succeeded;
        }

        public TTarget this[TKey key]
        {
            get { return this.convertAdapteeToTarget(this.AdapteeDictionary[key]); }
            set { this.AdapteeDictionary[key] = this.convertTargetToAdaptee(value); }
        }

        public ICollection<TKey> Keys
            => this.AdapteeDictionary.Keys;

        public ICollection<TTarget> Values
            => this.AdapteeDictionary.Values.Select(this.convertAdapteeToTarget).ToList();

        private KeyValuePair<TKey, TAdaptee> ConvertTargetToAdaptee(KeyValuePair<TKey, TTarget> item)
            => new KeyValuePair<TKey, TAdaptee>(item.Key, this.convertTargetToAdaptee(item.Value));

        private KeyValuePair<TKey, TTarget> ConvertAdapteeToTarget(KeyValuePair<TKey, TAdaptee> item)
            => new KeyValuePair<TKey, TTarget>(item.Key, this.convertAdapteeToTarget(item.Value));
    }
}
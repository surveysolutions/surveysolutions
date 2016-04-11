using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    public class TwoWayDictionaryAdapter<TKey, TAdaptee, TTarget> : IDictionary<TKey, TTarget>
    {
        private readonly IDictionary<TKey, TAdaptee> adapteeDictionary;
        private readonly Func<TAdaptee, TTarget> convertAdapteeToTarget; 
        private readonly Func<TTarget, TAdaptee> convertTargetToAdaptee; 

        public TwoWayDictionaryAdapter(
            IDictionary<TKey, TAdaptee> adapteeDictionary,
            Func<TAdaptee, TTarget> convertAdapteeToTarget,
            Func<TTarget, TAdaptee> convertTargetToAdaptee)
        {
            this.adapteeDictionary = adapteeDictionary;
            this.convertAdapteeToTarget = convertAdapteeToTarget;
            this.convertTargetToAdaptee = convertTargetToAdaptee;
        }

        public IEnumerator<KeyValuePair<TKey, TTarget>> GetEnumerator()
            => this.adapteeDictionary.Select(this.ConvertAdapteeToTarget).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable) this.adapteeDictionary).GetEnumerator();

        public void Add(KeyValuePair<TKey, TTarget> item)
            => this.adapteeDictionary.Add(this.ConvertTargetToAdaptee(item));

        public void Clear()
            => this.adapteeDictionary.Clear();

        public bool Contains(KeyValuePair<TKey, TTarget> item)
            => this.adapteeDictionary.Contains(this.ConvertTargetToAdaptee(item));

        public void CopyTo(KeyValuePair<TKey, TTarget>[] array, int arrayIndex)
            => this.adapteeDictionary.Select(this.ConvertAdapteeToTarget).ToList().CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<TKey, TTarget> item)
            => this.adapteeDictionary.Remove(this.ConvertTargetToAdaptee(item));

        public int Count
            => this.adapteeDictionary.Count;

        public bool IsReadOnly
            => this.adapteeDictionary.IsReadOnly;

        public void Add(TKey key, TTarget value)
            => this.adapteeDictionary.Add(key, this.convertTargetToAdaptee(value));

        public bool ContainsKey(TKey key)
            => this.adapteeDictionary.ContainsKey(key);

        public bool Remove(TKey key)
            => this.adapteeDictionary.Remove(key);

        public bool TryGetValue(TKey key, out TTarget value)
        {
            TAdaptee adapteeValue;
            bool succeeded = this.adapteeDictionary.TryGetValue(key, out adapteeValue);

            value = succeeded ? this.convertAdapteeToTarget(adapteeValue) : default(TTarget);

            return succeeded;
        }

        public TTarget this[TKey key]
        {
            get { return this.convertAdapteeToTarget(this.adapteeDictionary[key]); }
            set { this.adapteeDictionary[key] = this.convertTargetToAdaptee(value); }
        }

        public ICollection<TKey> Keys
            => this.adapteeDictionary.Keys;

        public ICollection<TTarget> Values
            => this.adapteeDictionary.Values.Select(this.convertAdapteeToTarget).ToList();

        private KeyValuePair<TKey, TAdaptee> ConvertTargetToAdaptee(KeyValuePair<TKey, TTarget> item)
            => new KeyValuePair<TKey, TAdaptee>(item.Key, this.convertTargetToAdaptee(item.Value));

        private KeyValuePair<TKey, TTarget> ConvertAdapteeToTarget(KeyValuePair<TKey, TAdaptee> item)
            => new KeyValuePair<TKey, TTarget>(item.Key, this.convertAdapteeToTarget(item.Value));
    }
}
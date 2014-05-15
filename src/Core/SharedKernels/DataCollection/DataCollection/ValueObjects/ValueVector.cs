﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects
{
    public class ValueVector<T> : IList<T>
    {
        private readonly List<T> values;

        public ValueVector()
        {
            this.values = new List<T>();
        }

        public ValueVector(IEnumerable<T> values)
        {
            this.values = values.ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.values.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            var typed = obj as ValueVector<T>;
            if (typed == null)
                return false;

            return this.values.SequenceEqual(typed);
        }

        public override int GetHashCode()
        {
            if (this.values != null)
            {
                unchecked
                {
                    return this.values.Aggregate(17, (current, item) => current * 23 + (item.GetHashCode()));
                }
            }
            return 0;
        }

        public int Length { get { return this.values.Count; }}

        public int IndexOf(T item)
        {
            return values.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            values.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            values.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return this.values[index]; }
            set { this.values[index] = value; }
        }

        public void Add(T item)
        {
            this.values.Add(item);
        }

        public void Clear()
        {
            this.values.Clear();
        }

        public bool Contains(T item)
        {
            return values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.values.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return this.values.Remove(item);
        }

        public int Count { get { return values.Count; } }

        public bool IsReadOnly { get { return false; }}

        public override string ToString()
        {
            if (!this.values.Any())
            {
                return Empty;
            }
            return string.Join(",", this.values);
        }

        public static implicit operator ValueVector<T>(T[] values)
        {
            return new ValueVector<T>(values);
        }

        public static implicit operator ValueVector<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (value == Empty)
            {
                return new ValueVector<T>();
            }

            var values = value.Split(',');

            var result = values.Select(v => (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromString(v)).ToList();

            return new ValueVector<T>(result);
        }

        private const string Empty = "Empty";
    }
}

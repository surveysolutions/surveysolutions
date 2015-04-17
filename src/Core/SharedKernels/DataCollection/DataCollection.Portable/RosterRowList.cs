using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace WB.Core.SharedKernels.DataCollection
{
    public class RosterRowList<T> : IEnumerable<T> where T : class, IRosterLevel 
    {
        private readonly Dictionary<string, T> internalDictionary = new Dictionary<string, T>();

        public RosterRowList(IEnumerable<IExpressionExecutable> rows)
        {
            if (rows != null)
                internalDictionary = rows.Select(r => r as T).ToDictionary(x => DecimalValueToString(x.@rowcode), x => x);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return internalDictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        /* In mono 0 and 0.0m is the same number, but they have different hashcodes. 
         * This code is workaround for it. 
         * Don't remove it.
         */
        private string DecimalValueToString(decimal decimalValue)
        {
            return decimalValue.ToString("F0", CultureInfo.InvariantCulture);
        }

        public T this[decimal code] { get { return internalDictionary[DecimalValueToString(code)]; } }

        public T ByIndex(int index)
        {
            return internalDictionary.Values.FirstOrDefault(v => v.rowindex == index);
        }
    }
}

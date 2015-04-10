using System;
using System.Collections;
using System.Collections.Generic;
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

        private string DecimalValueToString(decimal decimalValue)
        {
            if (decimalValue == 0)
            {
                return "0";
            }
            var possibleSeparators = new string[] { ",", "." };

            var decimalString = decimalValue.ToString();

            if (!possibleSeparators.Any(separator => decimalString.Contains(separator)))
                return decimalString;

            decimalString = decimalString.TrimEnd('0');
            decimalString = decimalString.TrimEnd(',', '.');
            return decimalString;
        }

        public T this[decimal code] { get { return internalDictionary[DecimalValueToString(code)]; } }

        public T ByIndex(int index)
        {
            return internalDictionary.Values.FirstOrDefault(v => v.rowindex == index);
        }
    }
}

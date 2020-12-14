using System;
using System.Collections.Generic;
using System.Linq;

namespace StatData.Core
{
    /// <summary>This class is responsible for storing and manipulating extended missing values.</summary>
    public class ExtendedMissingValuesFacility: IExtendedMissingValuesFacility
    {
        private readonly List<ExtendedMissingValue> _valuesList = new List<ExtendedMissingValue>();

        /// <summary> Declares an extended numeric missing value in the dataset with a given value and label </summary>
        /// <param name="value">Value in raw data, which should be interpreted as extended missing value.</param>
        /// <param name="label">Value label that must be associated with this extended missing value.</param>
        public void Add(string value, string label)
        {
            if (IndexOf(value) > -1)
                throw new Exception("Duplicate value: " + value);

            var extMiss = new ExtendedMissingValue
            {
                MissingValue = value,
                Label = label
            };
            _valuesList.Add(extMiss);
        }

        /// <summary> Probes if a particular string value is declared as an extended missing value. </summary>
        /// <param name="value">Value to probe for.</param>
        /// <returns>Returns true if the value is declared as an extended missing value.</returns>
        public bool IsMissing(string value)
        {
            return _valuesList.Any(emv => emv.MissingValue == value);
        }

        /// <summary> Returns index of the extended missing value. </summary>
        /// <param name="value">Extended missing value.</param>
        /// <returns>Index of the specified value.
        /// 
        ///  Returns -1 if the value is not declared as an extended numeric missing value.
        /// </returns>
        public int IndexOf(string value)
        {
            var pos = -1;
            foreach (var mv in _valuesList)
            {
                pos++;
                if (mv.MissingValue == value)
                    return pos;
            }
            return -1;
        }

        /// <summary> Obtain declared extended numeric missing values.</summary>
        /// <returns>declared extended missing values.</returns>
        public List<ExtendedMissingValue> GetList()
        {
            return _valuesList;
        }
    }
}

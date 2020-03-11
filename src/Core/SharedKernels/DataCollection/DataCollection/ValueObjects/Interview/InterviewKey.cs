using System;
using System.Text;
using System.Text.RegularExpressions;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public class InterviewKey
    {
        public int RawValue { get; }

        public InterviewKey(int rawValue)
        {
            this.RawValue = rawValue;
        }

        public override string ToString()
        {
            string nonFormattedId = this.RawValue.ToString("D");
            if(nonFormattedId.Length < 8) // old interview key generator used 8 digits and appended leading 00-00 values. Want to keep such behaviour
            {
                string missingZeros = new string('0', 8 - nonFormattedId.Length);
                nonFormattedId = missingZeros + nonFormattedId;
            }

            StringBuilder resultBuilder = new StringBuilder();

            int charsPrepended = 0;
            for (int i = nonFormattedId.Length - 1; i >= 0; i--)
            {
                resultBuilder.Insert(0, nonFormattedId[i]);
                charsPrepended++;
                if (i != 0 && charsPrepended % 2 == 0)
                {
                    resultBuilder.Insert(0, '-');
                }
            }

            return resultBuilder.ToString();
        }

        public static InterviewKey Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new InterviewKey(int.Parse(value.Replace("-", "")));
        }

        protected bool Equals(InterviewKey other)
        {
            return this.RawValue == other.RawValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InterviewKey)obj);
        }

        public override int GetHashCode()
        {
            return this.RawValue;
        }
    }
}

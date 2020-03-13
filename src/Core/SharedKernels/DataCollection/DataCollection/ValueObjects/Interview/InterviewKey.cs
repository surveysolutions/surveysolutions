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
            return RawValue <= 99_99_99_99
                ? RawValue.ToString("00-00-00-00")
                : RawValue.ToString("00-00-00-00-00");
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

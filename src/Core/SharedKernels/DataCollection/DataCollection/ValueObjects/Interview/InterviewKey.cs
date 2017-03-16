using System;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public class InterviewKey
    {
        public int RawValue { get; }

        public InterviewKey(int rawValueValue)
        {
            this.RawValue = rawValueValue;
        }

        public override string ToString()
        {
            return this.RawValue.ToString("00-00-00-00");
        }

        public static InterviewKey Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new InterviewKey(int.Parse(value.Replace("-", "")));
        }
    }
}
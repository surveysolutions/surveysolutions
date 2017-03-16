using System;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public class InterviewKey
    {
        public int Key { get; }

        public InterviewKey(int keyValue)
        {
            this.Key = keyValue;
        }

        public override string ToString()
        {
            return Key.ToString("00-00-00-00");
        }

        public static InterviewKey Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return new InterviewKey(int.Parse(value.Replace("-", "")));
        }
    }
}
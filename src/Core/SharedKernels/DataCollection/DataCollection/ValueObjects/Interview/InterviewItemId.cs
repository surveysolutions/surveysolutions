using System;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public struct InterviewItemId
    {
        public InterviewItemId(Guid id, decimal[] rosterVector):this()
        {
            Id = id;
            this.InterviewItemRosterVector = rosterVector ?? new decimal[0];
        }

        public InterviewItemId(Guid id)
            : this(id, new decimal[0]) { }

        public Guid Id;

        public decimal[] InterviewItemRosterVector { get; set; }

        public bool CompareWithVector(decimal[] vector)
        {
            if (this.InterviewItemRosterVector.Length != vector.Length)
                return false;
            return !this.InterviewItemRosterVector.Where((t, i) => t != vector[i]).Any();
        }

        public bool IsTopLevel()
        {
            return this.InterviewItemRosterVector.Length == 0;
        }

        public override bool Equals(object obj)
        {
            return obj is InterviewItemId && this == (InterviewItemId)obj;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(InterviewItemId x, InterviewItemId y)
        {
            if (x.Id != y.Id)
                return false;
            return x.CompareWithVector(y.InterviewItemRosterVector);
        }

        public static bool operator !=(InterviewItemId x, InterviewItemId y)
        {
            return !(x == y);
        }

        public override string ToString()
        {
            if (!IsTopLevel())
            {
                string vector = string.Join(",", this.InterviewItemRosterVector.Select(DecimalValueToString));
                return string.Format("{0},{1}", vector, Id);
            }
            return Id.ToString();
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

        /// <remarks>Is needed for Newtonsoft JSON.</remarks>
        public static explicit operator InterviewItemId(string b)
        {
            return Parse(b);
        }

        public static InterviewItemId Parse(string value)
        {
            if (value.Contains(","))
            {
                var items = value.Split(',');
                var vector = new decimal[items.Length - 1];
                for (int i = 0; i < items.Length - 1; i++)
                {
                    vector[i] = Convert.ToDecimal(items[i]);
                }
                return new InterviewItemId(Guid.Parse(items[items.Length - 1]), vector);
            }
            return new InterviewItemId(Guid.Parse(value));
        }
    }

}
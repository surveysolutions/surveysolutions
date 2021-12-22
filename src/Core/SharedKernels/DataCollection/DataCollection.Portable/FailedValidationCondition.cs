using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class FailedValidationCondition: IEquatable<FailedValidationCondition>
    {
        public FailedValidationCondition()
        {
        }

        public FailedValidationCondition(int failedConditionIndex)
        {
            this.FailedConditionIndex = failedConditionIndex;
        }

        public int FailedConditionIndex { get; set; }

        public bool Equals(FailedValidationCondition other)
        {
            if (other == null)
                return false;

            return this.FailedConditionIndex == other.FailedConditionIndex;
        }

        public override bool Equals(object obj)
        {
            FailedValidationCondition other = obj as FailedValidationCondition;
            if (other == null)
                return false;

            return this.FailedConditionIndex == other.FailedConditionIndex;
        }

        public static bool operator ==(FailedValidationCondition left, FailedValidationCondition right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }

        public static bool operator !=(FailedValidationCondition left, FailedValidationCondition right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return this.FailedConditionIndex.GetHashCode();
        }

        public override string ToString() => this.FailedConditionIndex.ToString();
    }
}

namespace WB.Core.SharedKernels.DataCollection
{
    public class FailedValidationCondition
    {
        public FailedValidationCondition()
        {
        }

        public FailedValidationCondition(int failedConditionIndex)
        {
            this.FailedConditionIndex = failedConditionIndex;
        }

        public int FailedConditionIndex { get; set; }
    }
}
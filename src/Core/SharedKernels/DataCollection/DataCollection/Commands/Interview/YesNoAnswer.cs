namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public struct YesNoAnswer
    {
        public YesNoAnswer(decimal option, bool answer)
        {
            this.Option = option;
            this.Answer = answer;
        }

        public decimal Option { get; private set; }
        public bool Answer { get; private set; }
    }
}
namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class QuestionProperties
    {
        protected QuestionProperties()
        {
        }

        public QuestionProperties(bool hideInstructions)
        {
            this.HideInstructions = hideInstructions;
        }

        public bool HideInstructions { get; protected set; }
    }
}
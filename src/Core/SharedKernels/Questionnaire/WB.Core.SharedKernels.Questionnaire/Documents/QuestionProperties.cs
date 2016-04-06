namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class QuestionProperties
    {
        public bool HideInstructions { get; protected set; }

        public QuestionProperties(bool hideInstructions)
        {
            this.HideInstructions = hideInstructions;
        }
    }
}
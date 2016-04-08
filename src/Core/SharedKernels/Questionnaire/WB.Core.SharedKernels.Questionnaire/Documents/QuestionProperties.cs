namespace WB.Core.SharedKernels.QuestionnaireEntities
{
    public class QuestionProperties
    {
        protected QuestionProperties()
        {
        }

        public QuestionProperties(bool hideInstructions, bool useFormatting)
        {
            this.HideInstructions = hideInstructions;
            this.UseFormatting = useFormatting;
        }

        public bool HideInstructions { get; set; }
        public bool UseFormatting { get; set; }
    }
}
namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireHistorySettings
    {
        public QuestionnaireHistorySettings(int questionnaireChangeHistoryLimit)
        {
            this.QuestionnaireChangeHistoryLimit = questionnaireChangeHistoryLimit;
        }

        public int QuestionnaireChangeHistoryLimit { get; }
    }
}

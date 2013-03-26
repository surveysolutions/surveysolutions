using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

namespace CAPI.Android.Core.Model.ViewModel.Statistics
{
    public class StatisticsQuestionViewModel
    {
        public StatisticsQuestionViewModel(ItemPublicKey publicKey, ItemPublicKey parentPublicKey, string text, string answerString, string errorMessage)
        {
            PublicKey = publicKey;
            ParentKey = parentPublicKey;
            Text = text;
            AnswerString = answerString;
            ErrorMessage = errorMessage;
        }

        public ItemPublicKey PublicKey { get; private set; }
        public ItemPublicKey ParentKey { get; private set; }
        public string Text { get; private set; }
        public string AnswerString { get; protected set; }
        public string ErrorMessage { get; private set; }
    }
}
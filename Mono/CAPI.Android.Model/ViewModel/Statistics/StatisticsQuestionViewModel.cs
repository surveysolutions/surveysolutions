using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.Statistics
{
    public class StatisticsQuestionViewModel
    {
        public StatisticsQuestionViewModel(InterviewItemId publicKey, InterviewItemId parentPublicKey, string text, string answerString, string errorMessage)
        {
            PublicKey = publicKey;
            ParentKey = parentPublicKey;
            Text = text;
            AnswerString = answerString;
            ErrorMessage = errorMessage;
        }

        public InterviewItemId PublicKey { get; private set; }
        public InterviewItemId ParentKey { get; private set; }
        public string Text { get; private set; }
        public string AnswerString { get; protected set; }
        public string ErrorMessage { get; private set; }
    }
}
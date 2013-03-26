using System.Linq;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Statistics
{
    public class StatisticsViewFactory : IViewFactory<StatisticsInput, StatisticsViewModel>
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireView> _documentStorage;

        public StatisticsViewFactory(IDenormalizerStorage<CompleteQuestionnaireView> documentStorage)
        {
            this._documentStorage = documentStorage;
        }

        #region Implementation of IViewFactory<StatisticsInput,StatisticsViewModel>

        public StatisticsViewModel Load(StatisticsInput input)
        {
            var doc = this._documentStorage.GetByGuid(input.QuestionnaireId);
            var enabledQuestion =
                doc.FindQuestion(
                    q => q.Status.HasFlag(QuestionStatus.Enabled));
            
            var answered =
                enabledQuestion.Where(q => q.Status.HasFlag(QuestionStatus.Answered)).Select(
                    q =>
                    new StatisticsQuestionViewModel(q.PublicKey, CalculateScreen(doc, q.PublicKey),
                                                    q.Text,
                                                    q.AnswerString, "")).ToList();
            var invalid = enabledQuestion.Where(q => !q.Status.HasFlag(QuestionStatus.Valid)).Select(
                q =>
                new StatisticsQuestionViewModel(q.PublicKey, CalculateScreen(doc, q.PublicKey), q.Text,
                                                q.AnswerString, q.ValidationMessage)).ToList();

            var unanswered = enabledQuestion.Where(q => !q.Status.HasFlag(QuestionStatus.Answered)).Select(
                q =>
                new StatisticsQuestionViewModel(q.PublicKey, CalculateScreen(doc, q.PublicKey), q.Text,
                                                q.AnswerString, "")).ToList();

            var result = new StatisticsViewModel(doc.PublicKey, doc.Title,
                                                 doc.Status, doc.FindQuestion(q => true).Count(), unanswered,
                                                 answered, invalid);
            return result;
        }

        #endregion
        protected ItemPublicKey CalculateScreen(CompleteQuestionnaireView doc, ItemPublicKey key)
        {
            return
                doc.Screens.Select(s=>s.Value).OfType<QuestionnaireScreenViewModel>().FirstOrDefault(
                    s => s.Items.Any(i => i.PublicKey == key)).ScreenId;
        }
    }
}
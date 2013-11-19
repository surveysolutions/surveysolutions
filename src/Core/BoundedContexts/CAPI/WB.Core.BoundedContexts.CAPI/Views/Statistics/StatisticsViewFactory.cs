using System.Linq;
using Main.Core.View;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.Statistics
{
    public class StatisticsViewFactory : IViewFactory<StatisticsInput, StatisticsViewModel>
    {
        private readonly IReadSideRepositoryReader<InterviewViewModel> documentStorage;

        public StatisticsViewFactory(IReadSideRepositoryReader<InterviewViewModel> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        #region Implementation of IViewFactory<StatisticsInput,StatisticsViewModel>

        public StatisticsViewModel Load(StatisticsInput input)
        {
            var doc = this.documentStorage.GetById(input.QuestionnaireId);
            var enabledQuestion =
                doc.FindQuestion(
                    q => q.IsEnabled());
            
            var answered =
                enabledQuestion.Where(q => q.Status.HasFlag(QuestionStatus.Answered)).Select(
                    q =>
                    new StatisticsQuestionViewModel(q.PublicKey, this.CalculateScreen(doc, q.PublicKey),
                                                    q.Text,
                                                    q.AnswerString, "")).ToList();
            var invalid = enabledQuestion.Where(q => !q.Status.HasFlag(QuestionStatus.Valid)).Select(
                q =>
                new StatisticsQuestionViewModel(q.PublicKey, this.CalculateScreen(doc, q.PublicKey), q.Text,
                                                q.AnswerString, q.ValidationMessage)).ToList();

            var unanswered = enabledQuestion.Where(q => !q.Status.HasFlag(QuestionStatus.Answered)).Select(
                q =>
                new StatisticsQuestionViewModel(q.PublicKey, this.CalculateScreen(doc, q.PublicKey), q.Text,
                                                q.AnswerString, "")).ToList();

            var result = new StatisticsViewModel(doc.PublicKey, doc.Title,
                                                 doc.Status, doc.FindQuestion(q => true).Count(), unanswered,
                                                 answered, invalid);
            return result;
        }

        #endregion
        protected InterviewItemId CalculateScreen(InterviewViewModel doc, InterviewItemId key)
        {
            return
                doc.Screens.Select(s=>s.Value).OfType<QuestionnaireScreenViewModel>().FirstOrDefault(
                    s => s.Items.Any(i => i.PublicKey == key)).ScreenId;
        }
    }
}
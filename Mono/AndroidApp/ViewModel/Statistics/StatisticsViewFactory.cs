using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.EventHandlers;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.DenormalizerStorage;

namespace AndroidApp.ViewModel.Statistics
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
            var doc = this._documentStorage.Query().First();
            var answered =
                doc.Questions.Where(q => q.Value.Status.HasFlag(QuestionStatus.Answered)).Select(
                    q =>
                    new StatisticsQuestionViewModel(q.Value.PublicKey, CalculateScreen(doc, q.Value.PublicKey),
                                                    q.Value.Text,
                                                    q.Value.AnswerString, "")).ToList();
            var invalid = doc.Questions.Where(q => !q.Value.Status.HasFlag(QuestionStatus.Valid)).Select(
                q =>
                new StatisticsQuestionViewModel(q.Value.PublicKey, CalculateScreen(doc, q.Value.PublicKey), q.Value.Text,
                                                q.Value.AnswerString, "")).ToList();

            var unanswered = doc.Questions.Where(q => !q.Value.Status.HasFlag(QuestionStatus.Answered)).Select(
                q =>
                new StatisticsQuestionViewModel(q.Value.PublicKey, CalculateScreen(doc, q.Value.PublicKey), q.Value.Text,
                                                q.Value.AnswerString, "")).ToList();

            var result = new StatisticsViewModel(input.QuestionnaireId, doc.Title,
                                                 SurveyStatus.Initial, doc.Questions.Count(), unanswered,
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
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Events.Questionnaire.Completed;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Restoring.EventStapshoot;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class DashboardDenormalizer : IEventHandler<SnapshootLoaded>, IEventHandler<QuestionnaireStatusChanged>
    {
        private readonly IDenormalizerStorage<DashboardModel> _documentStorage;

        public DashboardDenormalizer(IDenormalizerStorage<DashboardModel> documentStorage)
        {
            _documentStorage = documentStorage;
        }

        #region Implementation of IEventHandler<in SnapshootLoaded>

        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (document == null)
                return;
            PropeedCompleteQuestionnaire(document);
        }

        #endregion
        protected void PropeedCompleteQuestionnaire( CompleteQuestionnaireDocument doc)
        {
            var dashboard = _documentStorage.GetByGuid(doc.Responsible.Id);
            if (dashboard == null)
            {
                dashboard = new DashboardModel();
                _documentStorage.Store(dashboard, doc.Responsible.Id);
            }
            var survey = dashboard.Surveys.FirstOrDefault(s => s.PublicKey == doc.TemplateId);
            var featuredItems = doc.Find<ICompleteQuestion>(q => q.Featured);
            if (survey == null)
            {
                survey = new DashboardSurveyItem(doc.TemplateId, doc.Title);
                dashboard.Surveys.Add(survey);
            }
            var item = new DashboardQuestionnaireItem(doc.PublicKey, doc.Status, featuredItems.Select(
              q =>
              new FeaturedItem(q.PublicKey, q.QuestionText,
                               q.GetAnswerString())).ToList());
            survey.AddItem(item);
        }

        #region Implementation of IEventHandler<in QuestionnaireStatusChanged>

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var dashboard = _documentStorage.GetByGuid(evnt.Payload.Responsible.Id);
            if (dashboard == null)
                return;
            var questionnaire =
                dashboard.Surveys.SelectMany(s => s.AllItems).FirstOrDefault(
                    q => q.PublicKey == evnt.EventSourceId);
            if (questionnaire == null)
                return;
            questionnaire.SetStatus(evnt.Payload.Status);
        }

        #endregion
    }
}
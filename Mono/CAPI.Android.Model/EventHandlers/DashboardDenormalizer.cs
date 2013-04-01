using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Events.Questionnaire.Completed;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Restoring.EventStapshoot;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class DashboardDenormalizer : IEventHandler<SnapshootLoaded>, IEventHandler<QuestionnaireStatusChanged>{
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
            var featuredItems = doc.Find<ICompleteQuestion>(q => q.Featured);
            var item = new DashboardQuestionnaireItem(doc.PublicKey, doc.Status, featuredItems.Select(
              q =>
              new FeaturedItem(q.PublicKey, q.QuestionText,
                               q.GetAnswerString())).ToList());
            AddToDashboard(item, doc.Responsible.Id, doc.TemplateId, doc.Title);
        }

        protected void TryToRemoveFromOtherDashboards(Guid questionnarieKey, Guid template, Guid owner)
        {
            foreach (var dashboard in _documentStorage.Query().Where(d=>d.OwnerKey!=owner))
            {
                var survey = dashboard.GetSurvey(template);
                if (survey != null)
                {
                    if(survey.Remove(questionnarieKey))
                        return;
                }
            }
        }

        protected void AddToDashboard(DashboardQuestionnaireItem item, Guid dashbordOwner, Guid templateKey, string templateTitle)
        {
            TryToRemoveFromOtherDashboards(item.PublicKey, templateKey, dashbordOwner);

            var dashboard = _documentStorage.GetByGuid(dashbordOwner);
            if (dashboard == null)
            {
                dashboard = new DashboardModel(dashbordOwner);
                _documentStorage.Store(dashboard, dashbordOwner);
            }
            var survey = dashboard.Surveys.FirstOrDefault(s => s.PublicKey == templateKey);
            if (survey == null)
            {
                survey = new DashboardSurveyItem(templateKey, templateTitle);
                dashboard.Surveys.Add(survey);
            }
           
            survey.AddItem(item);

        }

        #region Implementation of IEventHandler<in QuestionnaireStatusChanged>

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var dashboard = _documentStorage.GetByGuid(evnt.Payload.Responsible.Id);
            if (dashboard == null)
                return;
            foreach (var dashboardSurveyItem in dashboard.Surveys)
            {
                var questionnarie = dashboardSurveyItem[evnt.EventSourceId];
                if (questionnarie != null)
                {
                    questionnarie.SetStatus(evnt.Payload.Status);
                    return;
                }
                /* if (dashboardSurveyItem.TryToChangeQuestionnaireState(evnt.EventSourceId, evnt.Payload.Status))
                    break;*/
            }
        }

        #endregion
        /*
        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var dashboard = _documentStorage.GetByGuid(evnt.Payload.PreviousResponsible.Id);
            if (dashboard == null)
                return;
            foreach (var dashboardSurveyItem in dashboard.Surveys)
            {
                var item = dashboardSurveyItem.GetItem(evnt.EventSourceId);
                if (item != null)
                {
                    dashboardSurveyItem.Remove(evnt.EventSourceId);
                    AddToDashboard(item, evnt.Payload.Responsible.Id, dashboardSurveyItem.PublicKey, dashboardSurveyItem.SurveyTitle);
                    break;
                }
            }
        }*/
    }
}
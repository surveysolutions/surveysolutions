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
using AndroidApp.Core.Model.ViewModel.Dashboard;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Complete;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Restoring.EventStapshoot;

namespace AndroidApp.Core.Model.EventHandlers
{
    public class DashboardDenormalizer : IEventHandler<SnapshootLoaded>
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
      //      var view = new DashboardModel();
            /* List<DashboardQuestionnaireItem> items = new List<DashboardQuestionnaireItem>();
       
         for (int i = 0; i < 5; i++)
         {
             var properties = new List<FeaturedItem>();
             for (int j = 0; j < 4; j++)
             {
                 properties.Add(new FeaturedItem(Guid.NewGuid(),"pTitle" + j,"p"+j));
             }
             var item = new DashboardQuestionnaireItem(Guid.NewGuid(),"status" + i, properties);
             items.Add(item);

         }
         var retval =
             new DashboardModel(
                 new[]
                     {
                         new DashboardSurveyItem(Guid.NewGuid(), "Super Servey1", items),
                         new DashboardSurveyItem(Guid.NewGuid(), "Super Servey2",  items),
                          new DashboardSurveyItem(Guid.NewGuid(), "Super Servey3", items)
                     });
         return retval;*/
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
            var currentItem = survey.Items.FirstOrDefault(i => i.PublicKey == doc.PublicKey);
            if (currentItem != null)
                survey.Items.Remove(currentItem);
            var item = new DashboardQuestionnaireItem(doc.PublicKey, doc.Status.Name, featuredItems.Select(
                q =>
                new FeaturedItem(q.PublicKey, q.QuestionText,
                                 q.GetAnswerString())).ToList());
            survey.Items.Add(item);
        }
    }
}
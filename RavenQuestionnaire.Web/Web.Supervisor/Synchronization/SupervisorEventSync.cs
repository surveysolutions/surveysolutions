using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.File;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.User;

namespace Web.Supervisor.Synchronization
{
    public class SupervisorEventSync : AbstractEventSync
    {
         private readonly IViewRepository viewRepository;
         private readonly IEventStore myEventStore;
         public SupervisorEventSync(IViewRepository viewRepository)
         {
             this.viewRepository = viewRepository;
             this.myEventStore = NcqrsEnvironment.Get<IEventStore>();

             if (myEventStore == null)
                 throw new Exception("IEventStore is not correct.");
         }

        #region Overrides of AbstractEventSync

         public override IEnumerable<AggregateRootEvent> ReadEvents()
         {
             List<AggregateRootEvent> retval = new List<AggregateRootEvent>();
             AddCompleteQuestionnairesInitState(retval);
             AddQuestionnairesTemplates(retval);
             AddUsers(retval);
             AddFiles(retval);
             return retval.OrderBy(x => x.EventTimeStamp).ToList();
         }

        #endregion

        protected void AddCompleteQuestionnairesInitState(List<AggregateRootEvent> retval)
        {
            var model =
                 viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(
                     new CompleteQuestionnaireBrowseInputModel());
            foreach (var item in model.Items)
            {
                if (item.Status.Name != SurveyStatus.Initial.Name)
                    continue;
                GetEventStreamById(retval, Guid.Parse(item.CompleteQuestionnaireId));
            }
        }
        protected void AddQuestionnairesTemplates(List<AggregateRootEvent> retval)
        {
              var model =
                 viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                     new QuestionnaireBrowseInputModel());
              foreach (var item in model.Items)
              {
                  GetEventStreamById(retval, Guid.Parse(item.Id));
              }
        }
        protected void AddUsers(List<AggregateRootEvent> retval)
        {
            var model =
               viewRepository.Load<UserBrowseInputModel, UserBrowseView>(
                   new UserBrowseInputModel());
            foreach (var item in model.Items)
            {
                GetEventStreamById(retval, Guid.Parse(item.Id));
            }
        }
        protected void AddFiles(List<AggregateRootEvent> retval)
        {
            var model =
               viewRepository.Load<FileBrowseInputModel, FileBrowseView>(
                   new FileBrowseInputModel());
            foreach (var item in model.Items)
            {
                GetEventStreamById(retval, Guid.Parse(item.FileName));
            }
        }
        protected void GetEventStreamById(List<AggregateRootEvent> retval, Guid aggregateRootId)
        {
            var events = myEventStore.ReadFrom(aggregateRootId,
                                                     int.MinValue, int.MaxValue);
            retval.AddRange(events.Select(e => new AggregateRootEvent(e)).ToList());
        }
    }
}

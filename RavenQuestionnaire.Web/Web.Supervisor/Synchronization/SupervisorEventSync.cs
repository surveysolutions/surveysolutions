using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Domain;
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
         private readonly IUnitOfWorkFactory unitOfWorkFactory;
         public SupervisorEventSync(IViewRepository viewRepository)
         {
             this.viewRepository = viewRepository;
             this.myEventStore = NcqrsEnvironment.Get<IEventStore>();
             this.unitOfWorkFactory = NcqrsEnvironment.Get<IUnitOfWorkFactory>();
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
                 if (!SurveyStatus.IsStatusAllowDownSupervisorSync(item.Status))
                     continue;
                 retval.AddRange(this.GetEventStreamById(item.CompleteQuestionnaireId, typeof(CompleteQuestionnaireAR)));
             }
         }

        protected void AddQuestionnairesTemplates(List<AggregateRootEvent> retval)
        {
              var model =
                 viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                     new QuestionnaireBrowseInputModel());
              foreach (var item in model.Items)
              {
                  retval.AddRange(this.GetEventStreamById(item.Id, typeof(QuestionnaireAR)));
              }
        }
        protected void AddUsers(List<AggregateRootEvent> retval)
        {
            var model =
               viewRepository.Load<UserBrowseInputModel, UserBrowseView>(
                   new UserBrowseInputModel());
            foreach (var item in model.Items)
            {
                retval.AddRange(this.GetEventStreamById(item.Id, typeof(UserAR)));
            }
        }
        protected void AddFiles(List<AggregateRootEvent> retval)
        {
            var model =
               viewRepository.Load<FileBrowseInputModel, FileBrowseView>(
                   new FileBrowseInputModel());
            foreach (var item in model.Items)
            {
                retval.AddRange(this.GetEventStreamById( Guid.Parse(item.FileName), typeof(FileAR)));
            }
        }

        public List<AggregateRootEvent> GetEventStreamById(Guid aggregateRootId, Type arType)
        {

            var events = this.myEventStore.ReadFrom(aggregateRootId,
                                                    int.MinValue, int.MaxValue);

            if (!events.Any())
                return new List<AggregateRootEvent>(0);
            var snapshotables = from i in arType.GetInterfaces()
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISnapshotable<>)
                                select i;
            if (!snapshotables.Any())
                return BuildEventStream(events);

            if (!typeof (SnapshootableAggregateRoot<>).MakeGenericType(
                    snapshotables.FirstOrDefault().GetGenericArguments()[0]).IsAssignableFrom(arType))
                return BuildEventStream(events);
            if (events.Last().Payload is SnapshootLoaded)
                return new List<AggregateRootEvent>()
                           {

                               new AggregateRootEvent(events.Last())
                           };
            AggregateRoot aggregateRoot;
            using (var unitOfWork = this.unitOfWorkFactory.CreateUnitOfWork(Guid.NewGuid()))
            {
                if (unitOfWork == null)
                    return BuildEventStream(events);
                aggregateRoot = unitOfWork.GetById(arType, aggregateRootId, null);
                if (aggregateRoot == null)
                    return BuildEventStream(events);
            }
            var snapshoot = arType.GetMethod("CreateSnapshot").Invoke(aggregateRoot, new object[0]);
                var eventSnapshoot = new SnapshootLoaded()
                                         {
                                             Template =
                                                 new Snapshot(aggregateRootId,
                                                              1,
                                                              snapshoot)
                                         };
                Guid commitId = Guid.NewGuid();
                Guid eventId = Guid.NewGuid();
                var uncommitedStream = new UncommittedEventStream(commitId);
                uncommitedStream.Append(new UncommittedEvent(eventId, aggregateRootId, aggregateRoot.Version + 1,
                                                             aggregateRoot.InitialVersion, DateTime.Now, eventSnapshoot,
                                                             events.Last().GetType().Assembly.GetName().Version));
                this.myEventStore.Store(uncommitedStream);
            
            return new List<AggregateRootEvent>()
                       {

                           new AggregateRootEvent(new CommittedEvent(commitId, eventId, aggregateRootId, 1,
                                                                     DateTime.Now, eventSnapshoot,
                                                                     events.Last().GetType().Assembly.GetName().Version))
                       };

        }
        private List<AggregateRootEvent> BuildEventStream(IEnumerable<CommittedEvent> events)
        {
           return events.Select(e => new AggregateRootEvent(e)).ToList();
        }
    }
}

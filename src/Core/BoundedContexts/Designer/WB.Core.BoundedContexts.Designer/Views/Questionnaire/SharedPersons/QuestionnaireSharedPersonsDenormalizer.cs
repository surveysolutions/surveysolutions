using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    internal class QuestionnaireSharedPersonsDenormalizer :
        IEventHandler<SharedPersonToQuestionnaireAdded>,
        IEventHandler<SharedPersonFromQuestionnaireRemoved>, IEventHandler
    {
        private readonly IReadSideRepositoryWriter<QuestionnaireSharedPersons> documentStorage;

        public QuestionnaireSharedPersonsDenormalizer(IReadSideRepositoryWriter<QuestionnaireSharedPersons> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        public void Handle(IPublishedEvent<SharedPersonToQuestionnaireAdded> evnt)
        {
            QuestionnaireSharedPersons item = this.documentStorage.GetById(evnt.EventSourceId) ??
                                                  new QuestionnaireSharedPersons(evnt.EventSourceId);

            if (item.SharedPersons.All(x => x.Id != evnt.Payload.PersonId))
            {
                item.SharedPersons.Add(new SharedPerson()
                {
                    Id = evnt.Payload.PersonId,
                    Email = evnt.Payload.Email
                });
            }

            this.documentStorage.Store(item, evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<SharedPersonFromQuestionnaireRemoved> evnt)
        {
            QuestionnaireSharedPersons item = this.documentStorage.GetById(evnt.EventSourceId);
            if (item != null)
            {
                var sharedPerson = item.SharedPersons.FirstOrDefault(x => x.Id == evnt.Payload.PersonId);
                if (sharedPerson != null)
                {
                    item.SharedPersons.Remove(sharedPerson);
                }

                this.documentStorage.Store(item, evnt.EventSourceId);
            }
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] { typeof(QuestionnaireSharedPersons) }; }
        }
    }
}
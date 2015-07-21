using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    internal class QuestionnaireSharedPersonsDenormalizer :BaseDenormalizer,
        IEventHandler<SharedPersonToQuestionnaireAdded>,
        IEventHandler<SharedPersonFromQuestionnaireRemoved>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireSharedPersons> documentStorage;

        public QuestionnaireSharedPersonsDenormalizer(IReadSideKeyValueStorage<QuestionnaireSharedPersons> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        public override object[] Writers
        {
            get { return new object[] { documentStorage}; }
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
                    Email = evnt.Payload.Email,
                    ShareType = evnt.Payload.ShareType
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
    }
}
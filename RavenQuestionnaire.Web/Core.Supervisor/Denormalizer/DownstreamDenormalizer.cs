using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.File;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using Newtonsoft.Json;
using WB.Core.Infrastructure;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace Core.Supervisor.Denormalizer
{
    public class DownstreamDenormalizer : IEventHandler<NewUserCreated>, 
                                          IEventHandler<QuestionnaireAssignmentChanged>,
                                          IEventHandler<QuestionnaireStatusChanged>, 
                                          IEventHandler<FileUploaded>
    {
        private readonly ISynchronizationDataStorage storage;

        public DownstreamDenormalizer(ISynchronizationDataStorage storage)
        {
            this.storage = storage;
        }

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            storage.SaveUser(new UserDocument
                {
                    UserName = evnt.Payload.Name,
                    Password = evnt.Payload.Password,
                    PublicKey = evnt.Payload.PublicKey,
                    CreationDate = evnt.EventTimeStamp,
                    Email = evnt.Payload.Email,
                    IsLocked = evnt.Payload.IsLocked,
                    Supervisor = evnt.Payload.Supervisor,
                    Roles = new List<UserRoles>(evnt.Payload.Roles)
                });
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            storage.SaveQuestionnarie(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            storage.SaveQuestionnarie(evnt.EventSourceId);
        }

        public void Handle(IPublishedEvent<FileUploaded> evnt)
        {
          //  throw new NotImplementedException();
        }
    }
}

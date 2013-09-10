using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core.Events.User;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class UserDenormalizer : IEventHandler<NewUserCreated>, IEventHandler<UserChanged>
    {
        private readonly IReadSideRepositoryWriter<LoginDTO> documentStorage;

        public UserDenormalizer(IReadSideRepositoryWriter<LoginDTO> documentStorage)
        {

            this.documentStorage = documentStorage;
        }


        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            this.documentStorage.Store(new LoginDTO(evnt.EventSourceId, evnt.Payload.Name, evnt.Payload.Password, evnt.Payload.IsLocked),
                                   evnt.EventSourceId);
        }


        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            var user = this.documentStorage.GetById(evnt.EventSourceId);
            if(user==null)
                return;
            this.documentStorage.Store(new LoginDTO(evnt.EventSourceId, user.Login, evnt.Payload.PasswordHash, user.IsLocked),
                                    evnt.EventSourceId);
        }
    }
}
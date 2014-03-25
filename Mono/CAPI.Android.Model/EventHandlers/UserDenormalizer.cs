using System;
using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class UserDenormalizer : IEventHandler<NewUserCreated>, IEventHandler<UserChanged>, IEventHandler
    {
        private readonly IReadSideRepositoryWriter<LoginDTO> documentStorage;

        public UserDenormalizer(IReadSideRepositoryWriter<LoginDTO> documentStorage)
        {
            this.documentStorage = documentStorage;
        }


        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            this.documentStorage.Store(new LoginDTO(evnt.EventSourceId, evnt.Payload.Name.ToLower(), evnt.Payload.Password, evnt.Payload.IsLocked, evnt.Payload.Supervisor.Id),
                                   evnt.EventSourceId);
        }


        public void Handle(IPublishedEvent<UserChanged> evnt)
        {
            var user = this.documentStorage.GetById(evnt.EventSourceId);
            if(user==null)
                return;
            this.documentStorage.Store(new LoginDTO(evnt.EventSourceId, user.Login.ToLower(), evnt.Payload.PasswordHash, user.IsLocked, Guid.Parse(user.Supervisor)),
                                    evnt.EventSourceId);
        }

        public string Name
        {
            get { return typeof (UserDenormalizer).FullName; }
        }

        public Type[] UsesViews
        {
            get
            {
                return new Type[0];
            }
        }

        public Type[] BuildsViews
        {
            get
            {
                return new[]{typeof(LoginDTO)};
            }
        }
    }
}
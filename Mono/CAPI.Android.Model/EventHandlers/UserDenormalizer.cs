using System;
using Main.Core.Events.User;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class UserDenormalizer : IEventHandler<NewUserCreated>
    {
        private readonly IDenormalizerStorage<UserView> _documentStorage;

        public UserDenormalizer(IDenormalizerStorage<UserView> documentStorage)
        {
            _documentStorage = documentStorage;
        }

        #region Implementation of IEventHandler<in NewUserCreated>

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            _documentStorage.Store(
                new UserView(evnt.Payload.PublicKey, evnt.Payload.Name, evnt.Payload.Password, evnt.Payload.Email,
                             DateTime.Now, evnt.Payload.Roles, evnt.Payload.IsLocked, evnt.Payload.Supervisor,
                             Guid.NewGuid()), evnt.EventSourceId);
        }

        #endregion
    }
}
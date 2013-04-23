using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ProjectionStorage;
using Main.Core.Events.User;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class UserDenormalizer : IEventHandler<NewUserCreated>
    {
        private readonly IDenormalizerStorage<UserView> _documentStorage;

        public UserDenormalizer(IProjectionStorage persistanceStore, IDenormalizerStorage<UserView> documentStorage)
        {

            _documentStorage = documentStorage;
            var fromStore = persistanceStore.RestoreProjection<List<UserView>>(Guid.Empty);
            var userList = fromStore??
                           new List<UserView>();
            foreach (UserView userView in userList)
            {
                _documentStorage.Store(userView, userView.PublicKey);
            }
        }

        #region Implementation of IEventHandler<in NewUserCreated>

        public void Handle(IPublishedEvent<NewUserCreated> evnt)
        {
            _documentStorage.Store(
                new UserView(evnt.Payload.PublicKey, evnt.Payload.Name, evnt.Payload.Password,
                             evnt.Payload.Email,
                             DateTime.Now, evnt.Payload.Roles, evnt.Payload.IsLocked,
                             evnt.Payload.Supervisor,
                             Guid.NewGuid()), evnt.EventSourceId);
        }

        #endregion

         
    }
}
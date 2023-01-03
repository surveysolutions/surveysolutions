using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Internal;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class UserToDeviceService : IUserToDeviceService
    {
        private readonly IUserRepository userRepository;

        public UserToDeviceService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public string GetLinkedDeviceId(Guid authorizedUserId)
        {
            var user = userRepository.FindById(authorizedUserId);
            return user.Profile?.DeviceId;
        }

        public async Task LinkDeviceToUserAsync(Guid authorizedUserId, string deviceId)
        {
            var user = await userRepository.FindByIdAsync(authorizedUserId);
            if (!user.IsInRole(UserRoles.Interviewer) && !user.IsInRole(UserRoles.Supervisor))
            {
                throw new InvalidOperationException("Only interviewer or supervisor can be linked to device");
            }

            if (user.Profile != null
                && !string.IsNullOrWhiteSpace(user.Profile.DeviceId)
                && user.Profile.DeviceRegistrationDate.HasValue
                && !user.Profile.IsAllowRelink())
            {
                throw new InvalidOperationException("You must have approve from supervisor or headquarters to relink device");
            }

            if(user.WorkspaceProfile == null)
                user.Profile = new HqUserProfile();

            user.Profile.DeviceId = deviceId;
            user.Profile.DeviceRegistrationDate = DateTime.UtcNow;
            user.Profile.ResetAllowRelinkFlag();
        }
    }
}

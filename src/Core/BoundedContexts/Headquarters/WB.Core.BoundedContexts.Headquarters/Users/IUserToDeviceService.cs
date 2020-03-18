using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public interface IUserToDeviceService
    {
        string GetLinkedDeviceId(Guid authorizedUserId);
        Task LinkDeviceToUserAsync(Guid authorizedUserId, string deviceId);
    }
}

using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IAuthorizedUser
    {
        bool IsAdministrator { get; }
        bool IsHeadquarter { get; }
        bool IsSupervisor { get; }
        bool IsObserver { get; }
        Guid Id { get; }
        string UserName { get; }
        string DeviceId { get; }
        UserRoles Role { get; }
    }
}
using System;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteSupervisor
{
    public interface IDeleteSupervisorService
    {
        void DeleteSupervisor(Guid supervisorId);
    }
}
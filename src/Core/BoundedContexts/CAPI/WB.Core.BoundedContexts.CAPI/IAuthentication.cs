using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Capi
{
    public interface IAuthentication
    {
        UserLight CurrentUser { get; }
        
        Guid SupervisorId { get; }
        
        bool IsLoggedIn { get; }
        
        Task<bool> LogOnAsync(string userName, string password, bool wasPasswordHashed = false);

        void LogOff();
    }
}
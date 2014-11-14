using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Capi
{
    public interface IAuthentication
    {
        UserLight CurrentUser { get; }
        
        Guid SupervisorId { get; }
        
        bool IsLoggedIn { get; }
        Task<bool> LogOn(string userName, string password, bool wasPasswordHashed = false);
        void LogOff();
    }
}
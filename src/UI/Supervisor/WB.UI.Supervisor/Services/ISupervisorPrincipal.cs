using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Supervisor.Services
{
    public interface ISupervisorPrincipal : IPrincipal
    {
        new ISupervisorUserIdentity CurrentUserIdentity { get; }
    }
}

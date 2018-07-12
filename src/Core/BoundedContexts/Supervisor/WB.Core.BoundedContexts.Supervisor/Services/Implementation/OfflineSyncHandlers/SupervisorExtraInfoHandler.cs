using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorExtraInfoHandler : IHandleCommunicationMessage
    {
        private readonly IPrincipal principal;

        public SupervisorExtraInfoHandler(IPrincipal principal)
        {
            this.principal = principal;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<SupervisorIdRequest, SupervisorIdResponse>(GetSupervisorId);
        }

        private Task<SupervisorIdResponse> GetSupervisorId(SupervisorIdRequest request)
        {
            return Task.FromResult(new SupervisorIdResponse()
            {
                SupervisorId = principal.CurrentUserIdentity.UserId
            });
        }

    }
}

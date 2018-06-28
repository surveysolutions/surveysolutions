using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorQuestionnaireHandler
        :   IHandleCommunicationMessage
    {
        public Task<GetQuestionnaireListResponse> Handle(GetQuestionnaireListRequest message)
        {
            var result = new GetQuestionnaireListResponse
            {
                Questionnaires = {"questionnaireId#1"}
            };

            return Task.FromResult(result);
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetQuestionnaireListRequest, GetQuestionnaireListResponse>(Handle);
        }
    }
}

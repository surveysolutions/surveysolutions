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

        public Task<SendBigAmountOfDataResponse> Handle(SendBigAmountOfDataRequest request)
        {
            return Task.FromResult(new SendBigAmountOfDataResponse
            {
                Data = request.Data
            });
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetQuestionnaireListRequest, GetQuestionnaireListResponse>(Handle);
            requestHandler.RegisterHandler<SendBigAmountOfDataRequest, SendBigAmountOfDataResponse>(Handle);
        }
    }
}

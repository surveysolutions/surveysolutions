using System;
using System.Threading.Tasks;
using System.Net.Http;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Supervisor.Interviews.Implementation
{
    internal class HeadquartersInterviewReader : HeadquartersEntityReader, IHeadquartersInterviewReader
    {
        public HeadquartersInterviewReader(IJsonUtils jsonUtils, IHeadquartersSettings headquartersSettings, Func<HttpMessageHandler> messageHandler)
            : base(jsonUtils, headquartersSettings, messageHandler) { }

        public async Task<InterviewSynchronizationDto> GetInterviewByUri(Uri headquartersInterviewUri)
        {
            return await GetEntityByUri<InterviewSynchronizationDto>(headquartersInterviewUri).ConfigureAwait(false);
        }
    }
}
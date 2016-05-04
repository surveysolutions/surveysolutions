using System;
using System.Threading.Tasks;
using System.Net.Http;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Interviews.Implementation
{
    internal class HeadquartersInterviewReader : HeadquartersEntityReader, IHeadquartersInterviewReader
    {
        public HeadquartersInterviewReader(ISerializer serializer, IHeadquartersSettings headquartersSettings, Func<HttpMessageHandler> messageHandler)
            : base(serializer, headquartersSettings, messageHandler) { }

        public async Task<InterviewSynchronizationDto> GetInterviewByUri(Uri headquartersInterviewUri)
        {
            return await GetEntityByUri<InterviewSynchronizationDto>(headquartersInterviewUri).ConfigureAwait(false);
        }
    }
}
using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Interviews.Implementation
{
    internal class HeadquartersInterviewReader : HeadquartersEntityReader, IHeadquartersInterviewReader
    {
        public HeadquartersInterviewReader(IJsonUtils jsonUtils, IHeadquartersSettings headquartersSettings)
            : base(jsonUtils, headquartersSettings) {}

        public async Task<InterviewSynchronizationDto> GetInterviewByUri(Uri headquartersInterviewUri)
        {
            return await GetEntityByUri<InterviewSynchronizationDto>(headquartersInterviewUri).ConfigureAwait(false);
        }
    }
}
using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor.Interviews
{
    public interface IHeadquartersInterviewReader
    {
        Task<InterviewSynchronizationDto> GetInterviewByUri(Uri headquartersInterviewUri);
    }
}
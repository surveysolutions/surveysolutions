using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IAssignmentSynchronizationApi
    {
        Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken);
        Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken);
    }
}
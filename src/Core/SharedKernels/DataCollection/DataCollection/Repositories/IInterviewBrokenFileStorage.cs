using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories;

public interface IInterviewBrokenFileStorage : IInterviewFileStorage
{
    Task<InterviewBinaryDataDescriptor> FirstOrDefaultAsync();
}

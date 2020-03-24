using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IAudioAuditFileStorage : IInterviewFileStorage
    {
        Task<bool> HasAnyAudioAuditFilesStoredAsync(QuestionnaireIdentity questionnaire);
    }
}

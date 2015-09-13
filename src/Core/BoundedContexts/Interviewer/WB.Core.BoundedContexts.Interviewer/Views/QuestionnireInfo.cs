using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class QuestionnireInfo : IPlainStorageEntity
    {
        public string Id { get; set; }
        public bool AllowCensus { get; set; }
    }
}
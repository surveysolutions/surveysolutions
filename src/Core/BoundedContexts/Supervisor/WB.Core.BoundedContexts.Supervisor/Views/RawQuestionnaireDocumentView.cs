using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Views
{
    public class RawQuestionnaireDocumentView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Document { get; set; }
    }
}
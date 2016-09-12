using Main.Core.Documents;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class QuestionnaireDocumentView : IPlainStorageEntity
    {
        [PrimaryKey]
        public string Id { get; set; }
        public QuestionnaireDocument Document { get; set; }
    }
}

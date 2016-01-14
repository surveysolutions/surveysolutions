using Main.Core.Documents;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class QuestionnaireDocumentView : IPlainStorageEntity
    {
        public int OID { get; set; }
        public string Id { get; set; }
        public QuestionnaireDocument Document { get; set; }
    }
}

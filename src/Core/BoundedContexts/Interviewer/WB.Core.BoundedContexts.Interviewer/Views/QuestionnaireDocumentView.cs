using Main.Core.Documents;
using SQLite.Net.Attributes;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class QuestionnaireDocumentView : IPlainStorageEntity
    {
        [PrimaryKey, AutoIncrement]
        public int OID { get; set; }
        public string Id { get; set; }
        public QuestionnaireDocument Document { get; set; }
    }
}

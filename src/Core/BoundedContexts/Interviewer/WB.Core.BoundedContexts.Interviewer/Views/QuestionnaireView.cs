using SQLite.Net.Attributes;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class QuestionnaireView : IPlainStorageEntity
    {
        [PrimaryKey, AutoIncrement]
        public int OID { get; set; }
        public string Id { get; set; }
        public QuestionnaireIdentity Identity { get; set; }
        public string Title { get; set; }
        public bool Census { get; set; }
    }
}

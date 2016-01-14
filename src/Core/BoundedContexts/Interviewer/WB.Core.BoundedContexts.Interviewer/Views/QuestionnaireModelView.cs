using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class QuestionnaireModelView : IPlainStorageEntity
    {
        public int OID { get; set; }
        public string Id { get; set; }
        public QuestionnaireModel Model { get; set; }
    }
}

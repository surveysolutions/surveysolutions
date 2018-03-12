using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class QuestionnaireLookupTable
    {
        public QuestionnaireIdentity Id { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
    }
}

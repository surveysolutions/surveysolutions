using Sqo.Attributes;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Views
{
    public class QuestionnaireMetaInfoStorageViewModel : Entity
    {
        [Index]
        public string UserName { get; set; }
        public QuestionnaireMetaInfo MetaInfo { get; set; }
    }
}
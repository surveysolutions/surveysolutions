using Main.Core.Documents;
using Sqo.Attributes;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Views
{
    public class QuestionnaireStorageViewModel : Entity
    {
        [Document]
        public QuestionnaireDocument Questionnaire { get; set; }
    }
}
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Views
{
    public class Questionnaire : IPlainStorageEntity
    {
        public string Id { get; set; }
        public QuestionnaireDocument Document { get; set; }
        public string Assembly { get; set; }
    }
}

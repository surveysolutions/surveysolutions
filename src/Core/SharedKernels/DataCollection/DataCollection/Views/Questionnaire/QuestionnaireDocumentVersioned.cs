using Main.Core.Documents;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireDocumentVersioned : IView
    {
        public QuestionnaireDocument Questionnaire { get; set; }
        public long Version { get; set; }
    }
}

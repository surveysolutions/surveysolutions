using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireDocumentVersioned : IVersionedView
    {
        public QuestionnaireDocument Questionnaire { get; set; }
        public long Version { get; set; }
    }
}

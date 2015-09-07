using Main.Core.Documents;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class QuestionnaireApiView
    {
        public QuestionnaireDocument Document { get; set; }
        public bool AllowCensus { get; set; }
    }
}

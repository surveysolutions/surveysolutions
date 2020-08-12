using Main.Core.Documents;

// ReSharper disable once CheckNamespace
namespace WB.Core.SharedKernels.SurveySolutions.Api.Designer
{
    public class Questionnaire
    {
        public Questionnaire(QuestionnaireDocument document, string assembly)
        {
            Document = document;
            Assembly = assembly;
        }

        public QuestionnaireDocument Document { get; set; }
        public string Assembly { get; set; }
    }
}

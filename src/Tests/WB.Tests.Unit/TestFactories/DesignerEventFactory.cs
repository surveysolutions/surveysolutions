extern alias designer;
using designer::Main.Core.Events.Questionnaire;
using Main.Core.Documents;

namespace WB.Tests.Unit.TestFactories
{
    internal class DesignerEventFactory
    {
        public TemplateImported TemplateImported(QuestionnaireDocument questionnaireDocument)
        {
            return new TemplateImported { Source = questionnaireDocument };
        }
    }
}
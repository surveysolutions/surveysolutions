using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVersioner : IQuestionnaireVersioner
    {
        //New Era of c# conditions
        private static readonly QuestionnaireVersion version_5 = new QuestionnaireVersion(5, 0, 0);

        public QuestionnaireVersion GetVersion(QuestionnaireDocument questionnaire)
        {
            return version_5;
        }
    }
}
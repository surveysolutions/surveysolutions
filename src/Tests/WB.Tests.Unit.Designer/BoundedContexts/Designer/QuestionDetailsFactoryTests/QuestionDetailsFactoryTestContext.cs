using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionDetailsFactoryTests
{
    internal class QuestionDetailsFactoryTestContext
    {
        protected static QuestionDetailsViewMapper CreateQuestionDetailsFactory()
        {
            return new QuestionDetailsViewMapper();
        }
    }
}

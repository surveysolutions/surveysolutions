using System.Text;
using System.Threading.Tasks;
using Main.Core.Utility;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionDetailsFactoryTests
{
    internal class QuestionDetailsFactoryTestContext
    {
        protected static QuestionDetailsViewMapper CreateQuestionDetailsFactory()
        {
            return new QuestionDetailsViewMapper();
        }
    }
}

using NSubstitute;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Controllers.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.TranslationsControllerTests
{
    public class TranslationsControllerTestsContext
    {
        public static TranslationController CreateTranslationsController(DesignerDbContext dbContext = null, 
            IQuestionnaireViewFactory questionnaireViewFactory = null)
        {
            return new TranslationController(
                dbContext: dbContext ?? Create.InMemoryDbContext(),
                questionnaireViewFactory : questionnaireViewFactory ?? Substitute.For<IQuestionnaireViewFactory>());
        }
    }
}

using NSubstitute;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.TranslationsControllerTests
{
    public class TranslationsControllerTestsContext
    {
        public static TranslationController CreateTranslationsController(IPlainStorageAccessor<TranslationInstance> translations = null)
        {
            return new TranslationController(
                translations: translations ?? Substitute.For<IPlainStorageAccessor<TranslationInstance>>());
        }
    }
}
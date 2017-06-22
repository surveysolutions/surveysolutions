using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.TranslationsControllerTests
{
    public class when_getting_translations : TranslationsControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var translationsStorage = new InMemoryPlainStorageAccessor<TranslationInstance>();
            translationsStorage.Store(storedTranslations.Select(x=>new Tuple<TranslationInstance, object>(x, Guid.NewGuid())));

            controller = CreateTranslationsController(translations: translationsStorage);
            BecauseOf();
        }

        private void BecauseOf() => expectedTranslations = controller.Get(questionnaireId,  version: ApiVersion.CurrentTesterProtocolVersion);

        [NUnit.Framework.Test] public void should_return_only_1_translation_by_specified_questionnaire () =>
            expectedTranslations.Length.ShouldEqual(1);
        
        private static TranslationController controller;
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");

        private static TranslationDto[] expectedTranslations;

        private static readonly TranslationInstance[] storedTranslations =
        {
            Create.TranslationInstance(questionnaireId: questionnaireId),
            Create.TranslationInstance(questionnaireId: Guid.NewGuid())
        };
    }
}
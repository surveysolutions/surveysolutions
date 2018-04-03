using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;
using WB.UI.Designer.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.TranslationsControllerTests
{
    public class when_getting_translations : TranslationsControllerTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var translationsStorage = new InMemoryPlainStorageAccessor<TranslationInstance>();
            translationsStorage.Store(storedTranslations.Select(x => Tuple.Create<TranslationInstance, object>(x, x.Id)));

            var questionnaireDocument = new QuestionnaireDocument();

            questionnaireDocument.Translations = new[] {new Translation {Id = translationId, Name = translationName}}.ToList();
                

            var questionnaireView = Create.QuestionnaireView(questionnaireDocument);
            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == questionnaireView);

            controller = CreateTranslationsController(translations: translationsStorage, questionnaireViewFactory: questionnaireViewFactory);
            BecauseOf();
        }

        private void BecauseOf() => expectedTranslations = controller.Get(questionnaireId, version: ApiVersion.CurrentTesterProtocolVersion);

        [Test]
        public void should_return_only_1_translation_by_specified_questionnaire() =>
            expectedTranslations.Should().HaveCount(1);

        private static TranslationController controller;

        private static readonly Guid questionnaireId = Id.g1;
        private static readonly Guid translationId = Id.g2;
        private static readonly string translationName = "translation";


        private static TranslationDto[] expectedTranslations;

        private static readonly TranslationInstance[] storedTranslations =
        {
            Create.TranslationInstance(questionnaireId: questionnaireId, translationId:translationId),
            Create.TranslationInstance(questionnaireId: questionnaireId),
            Create.TranslationInstance(questionnaireId: Guid.NewGuid()),
            Create.TranslationInstance(questionnaireId: Guid.NewGuid())
        };
    }
}

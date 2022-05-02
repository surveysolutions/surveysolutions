using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Api.Tester.TranslationsControllerTests
{
    public class when_getting_translations : TranslationsControllerTestsContext
    {
        [Test]
        public async Task should_return_only_1_translation_by_specified_questionnaire()
        {
            var translationsStorage = Create.InMemoryDbContext();
            translationsStorage.TranslationInstances.AddRange(storedTranslations);
            translationsStorage.SaveChanges();

            var questionnaireDocument = new QuestionnaireDocument();

            questionnaireDocument.Translations = new[] {new Translation {Id = translationId, Name = "translation"}}.ToList();
                

            var questionnaireView = Create.QuestionnaireView(questionnaireDocument);
            var questionnaireViewFactory = Mock.Of<IQuestionnaireViewFactory>(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>()) == questionnaireView);

            var controller = CreateTranslationsController(dbContext: translationsStorage, questionnaireViewFactory: questionnaireViewFactory);

            // Act
            expectedTranslations = (TranslationDto[]) (await controller.Get(new QuestionnaireRevision(questionnaireId), version: ApiVersion.CurrentTesterProtocolVersion) as OkObjectResult)?.Value;

            // Assert
            Assert.That(expectedTranslations, Has.Length.EqualTo(1));
        }

        private static readonly Guid questionnaireId = Id.g1;
        private static readonly Guid translationId = Id.g2;

        private TranslationDto[] expectedTranslations;

        private static readonly TranslationInstance[] storedTranslations =
        {
            Create.TranslationInstance(questionnaireId: questionnaireId, translationId:translationId),
            Create.TranslationInstance(questionnaireId: questionnaireId),
            Create.TranslationInstance(questionnaireId: Guid.NewGuid()),
            Create.TranslationInstance(questionnaireId: Guid.NewGuid())
        };
    }
}

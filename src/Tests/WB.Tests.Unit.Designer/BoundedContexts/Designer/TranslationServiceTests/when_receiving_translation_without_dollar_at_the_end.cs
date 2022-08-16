using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Tests.Abc;
using WB.UI.Designer.Extensions;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    [TestFixture(Description = "KP-13585", TestOf = typeof(TranslationsService))]
    internal class when_receiving_translation_without_dollar_at_the_end : TranslationsServiceTestsContext
    {
        [Test]
        public void should_apply_translations_with_backward_compatibility()
        {
            var testType = typeof(when_storing_translations_from_excel_file);
            var readResourceFile = testType.Namespace + ".TranslationsForCascading.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);

            var plainStorageAccessor = Create.InMemoryDbContext();

            var parentQuestionId = Guid.Parse("61b4a82485f22e719968d38dfbd3bd6d");
            var childQuestionId = Guid.Parse("865f969620e583c30d13b013210e0b56");
            var questionnaire = Create.QuestionnaireDocument(
                children: new IComposite[]
            {
                Create.SingleOptionQuestion(questionId: parentQuestionId,
                    variable: "parent",
                    answerCodes: new []{1m, 2m, 3m}),
                Create.SingleOptionQuestion(childQuestionId,
                    cascadeFromQuestionId: parentQuestionId,
                    variable: "child",
                    answers: new List<Answer>
                    {
                        Create.Answer(value: 1, answer: "one_child", parentValue: 1),
                        Create.Answer(value: 2, answer: "two_child", parentValue: 2),
                        Create.Answer(value: 3, answer: "three_child", parentValue: 3)
                    })
            });
            var questionnaires = new Mock<IQuestionnaireViewFactory>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireView(questionnaire));

            var service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);

            service.Store(questionnaire.PublicKey, Id.g1, manifestResourceStream.ReadToEnd());

            var translationInstance = plainStorageAccessor.TranslationInstances.Where(x => x.Type == TranslationType.OptionTitle).ToList();

            Assert.That(translationInstance, Has.Count.EqualTo(6));

            Assert.That(translationInstance.Single(x => x.Value == "один родитель").TranslationIndex, Is.EqualTo("1"));
            Assert.That(translationInstance.Single(x => x.Value == "два родитель").TranslationIndex, Is.EqualTo("2"));
            Assert.That(translationInstance.Single(x => x.Value == "три родитель").TranslationIndex, Is.EqualTo("3$3"));

            Assert.That(translationInstance.Single(x => x.Value == "один ребёнок").TranslationIndex, Is.EqualTo("1$1"));
            Assert.That(translationInstance.Single(x => x.Value == "два ребёнка").TranslationIndex, Is.EqualTo("2"));
            Assert.That(translationInstance.Single(x => x.Value == "три ребёнка").TranslationIndex, Is.EqualTo("3"));
        }
    }
}

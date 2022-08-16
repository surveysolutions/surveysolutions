using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_storing_translations_from_excel_file : TranslationsServiceTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var testType = typeof(when_storing_translations_from_excel_file);
            var readResourceFile = testType.Namespace + ".testTranslations.xlsx";
            var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                manifestResourceStream.CopyTo(memoryStream);
                fileStream = memoryStream.ToArray();
            }

            plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaire = Create.QuestionnaireDocument(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
            {
                Create.Group(groupId: Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")),
                Create.SingleQuestion(id: questionId, variable: "question", isFilteredCombobox: true, options: new List<Answer> {Create.Option("1", "Combobox Option")})
            });

            var questionnaires = new Mock<IQuestionnaireViewFactory>();
            questionnaires.SetReturnsDefault(Create.QuestionnaireView(questionnaire));

            service = Create.TranslationsService(plainStorageAccessor, questionnaires.Object);
            BecauseOf();
        }

        private void BecauseOf() => service.Store(questionnaireId, translationId, fileStream);

        [Test]
        public void should_store_all_entities_for_questionnaire_and_culture() =>
            plainStorageAccessor.TranslationInstances.All(x => x.QuestionnaireId == questionnaireId && x.TranslationId == translationId).Should().BeTrue();

        [Test]
        public void should_store_title_translation()
        {
            var translationInstance = plainStorageAccessor.TranslationInstances.Single(x => x.Type == TranslationType.Title);
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("title");
            translationInstance.TranslationIndex.Should().BeNull();
        }

        [Test]
        public void should_store_instruction_translation()
        {
            var translationInstance = plainStorageAccessor.TranslationInstances.Single(x => x.Type == TranslationType.Instruction);
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("instruction");
            translationInstance.TranslationIndex.Should().BeNull();
        }

        [Test]
        public void should_store_validation_translation()
        {
            var translationInstance = plainStorageAccessor.TranslationInstances.Single(x => x.Type == TranslationType.ValidationMessage);
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("validation message");
            translationInstance.TranslationIndex.Should().Be("1");
        }


        [Test]
        public void should_store_option_translation()
        {
            var translationInstance = plainStorageAccessor.TranslationInstances.First(x => x.Type == TranslationType.OptionTitle);
            translationInstance.QuestionnaireEntityId.Should().Be(entityId);
            translationInstance.Value.Should().Be("option");
            translationInstance.TranslationIndex.Should().Be("2");
        }

        [Test]
        public void should_store_option_translation_for_combobox_from_second_sheet()
        {
            var translationInstance = plainStorageAccessor.TranslationInstances.Last(x => x.Type == TranslationType.OptionTitle);
            translationInstance.QuestionnaireEntityId.Should().Be(questionId);
            translationInstance.Value.Should().Be("Опция Комбобокса");
            translationInstance.TranslationIndex.Should().Be("1");
        }

        TranslationsService service;
        DesignerDbContext plainStorageAccessor;
        byte[] fileStream;
        Guid questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        Guid entityId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        Guid questionId = Guid.Parse("11111111111111111111111111111111");
        Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}

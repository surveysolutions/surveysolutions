using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.TranslationServiceTests
{
    internal class when_storing_transation_for_different_questionnaire : TranslationsServiceTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaire = Create.QuestionnaireDocument(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), children: new IComposite[]
            {
                Create.Group(groupId: Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"))
            });
            translationId = Guid.Parse("11111111111111111111111111111111");
            questionnaire.Translations.Add(Create.Translation(translationId));

            var testType = typeof(when_storing_translations_from_excel_file_with_missing_same_translations);
            var readResourceFile = $"{testType.Namespace}.translationsForDifferentQuestionnaire.xlsx";
            using (var manifestResourceStream = testType.Assembly.GetManifestResourceStream(readResourceFile))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    manifestResourceStream.CopyTo(memoryStream);
                    fileStream = memoryStream.ToArray();
                }
            }

            plainStorageAccessor = Create.InMemoryDbContext();

            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaires.SetReturnsDefault(questionnaire);
            service = Create.TranslationsService(plainStorageAccessor,
                questionnaireStorage: questionnaires.Object);
            BecauseOf();
        }

        private void BecauseOf() => service.Store(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), translationId, fileStream);

        [Test] 
        public void should_not_store_entities_from_other_questionnare() 
            => plainStorageAccessor.TranslationInstances.Count().Should().Be(0);

        byte[] fileStream;
        DesignerDbContext plainStorageAccessor;
        TranslationsService service;
        Guid translationId;
    }
}

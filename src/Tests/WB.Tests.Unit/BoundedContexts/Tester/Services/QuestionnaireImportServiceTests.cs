using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.Services
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireImportService))]
    public class QuestionnaireImportServiceTests
    {
        [Test]
        public void when_importing_a_revision_should_preserve_other_revision_translations()
        {
            var questionnaireId = Guid.NewGuid();
            var previous = new QuestionnaireIdentity(questionnaireId, 1);
            var current = new QuestionnaireIdentity(questionnaireId, 2);
            var translations = Create.Storage.InMemorySqlitePlainStorage<TranslationInstance>();
            var oldTranslation = new TranslationInstance { Id = Guid.NewGuid().ToString(), QuestionnaireId = previous.ToString() };
            translations.Store(oldTranslation);
            var service = new QuestionnaireImportService(Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IQuestionnaireAssemblyAccessor>(), Mock.Of<IOptionsRepository>(), translations);

            service.ImportQuestionnaire(current, Create.Entity.QuestionnaireDocumentWithOneChapter(),
                "assembly", Array.Empty<TranslationDto>(), Array.Empty<ReusableCategoriesDto>());

            translations.GetById(oldTranslation.Id).Should().NotBeNull();
        }
    }
}

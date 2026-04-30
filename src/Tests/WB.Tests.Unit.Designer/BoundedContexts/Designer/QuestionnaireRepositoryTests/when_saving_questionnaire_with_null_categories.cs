using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Repositories;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireRepositoryTests
{
    [TestOf(typeof(QuestionnaireRepository))]
    internal class when_saving_questionnaire_with_null_categories
    {
        [OneTimeSetUp]
        public void context()
        {
            var storage = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            storage
                .Setup(s => s.Store(It.IsAny<QuestionnaireDocument>(), It.IsAny<string>()))
                .Callback<QuestionnaireDocument, string>((questionnaire, _) => storedQuestionnaire = questionnaire);

            var document = Create.QuestionnaireDocument(Id.g1);
            document.Categories = new List<Categories>
            {
                new Categories { Id = Id.g2, Name = "Category 1" },
                null,
                new Categories { Id = Id.g3, Name = "Category 2" }
            };

            var questionnaire = Create.Questionnaire(Id.gA, document);
            originalCategories = questionnaire.QuestionnaireDocument.Categories;

            var options = new DbContextOptionsBuilder<DesignerDbContext>()
                .UseInMemoryDatabase(databaseName: nameof(when_saving_questionnaire_with_null_categories))
                .Options;

            using var dbContext = new DesignerDbContext(options);
            var repository = new QuestionnaireRepository(storage.Object, new Mock<System.IServiceProvider>().Object, dbContext);

            repository.Save(questionnaire);
        }

        [Test]
        public void should_remove_null_items_before_storing_document()
        {
            storedQuestionnaire.Should().NotBeNull();
            storedQuestionnaire.Categories.Should().HaveCount(2);
            storedQuestionnaire.Categories[0].Id.Should().Be(Id.g2);
            storedQuestionnaire.Categories[1].Id.Should().Be(Id.g3);
        }

        [Test]
        public void should_not_mutate_aggregate_document_categories()
        {
            originalCategories.Should().HaveCount(3);
            originalCategories.Should().Contain(category => category == null);
        }

        private static List<Categories> originalCategories;
        private static QuestionnaireDocument storedQuestionnaire;
    }
}

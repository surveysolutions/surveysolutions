using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.QuestionnaireTests
{
    [TestOf(typeof(QuestionnaireDocument))]
    internal class when_cloning_questionnaire_document_with_null_category_item
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter();
            questionnaire.Categories = new List<Categories>
            {
                new Categories { Id = Id.g1, Name = "Category 1" },
                null,
                new Categories { Id = Id.g2, Name = "Category 2" },
                null
            };

            clonedQuestionnaire = questionnaire.Clone();
        }

        [Test]
        public void should_remove_null_item_from_categories_list()
        {
            clonedQuestionnaire.Categories.Should().HaveCount(2);
            clonedQuestionnaire.Categories[0].Id.Should().Be(Id.g1);
            clonedQuestionnaire.Categories[1].Id.Should().Be(Id.g2);
        }

        private static QuestionnaireDocument clonedQuestionnaire;
    }
}

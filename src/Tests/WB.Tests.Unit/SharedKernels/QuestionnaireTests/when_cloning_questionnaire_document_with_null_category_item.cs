using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.QuestionnaireTests
{
    internal class when_cloning_questionnaire_document_with_null_category_item
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter();
            questionnaire.Categories = new List<Categories>
            {
                new Categories { Id = Id.g1, Name = "Category 1" },
                null
            };

            clonedQuestionnaire = questionnaire.Clone();
        }

        [Test]
        public void should_remove_null_item_from_categories_list()
        {
            clonedQuestionnaire.Categories.Should().ContainSingle();
            clonedQuestionnaire.Categories[0].Id.Should().Be(Id.g1);
        }

        private static QuestionnaireDocument clonedQuestionnaire;
    }
}

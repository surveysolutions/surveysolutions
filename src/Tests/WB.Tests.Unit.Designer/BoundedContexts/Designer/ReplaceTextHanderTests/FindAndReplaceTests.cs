using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Tests.Abc;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    [TestFixture]
    internal class FindAndReplaceTests : QuestionnaireTestsContext
    {
        [Test]
        public void when_searching_texts_in_option_filter_expression_in_categorical__linked_question()
        {
            string searchFor = "to_search";
            Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid filteredQuestionId = Id.g2;
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId, groupId: chapterId);
            questionnaire.AddTextListQuestion(Id.g1, chapterId, responsibleId);
            questionnaire.AddMultiOptionQuestion(filteredQuestionId, chapterId, responsibleId,
                linkedToQuestionId: Id.g1, optionsFilterExpression: $"filter with {searchFor}");

            var matches = questionnaire.FindAllTexts(searchFor, false, false, false);

            Assert.That(matches.Any(x => x.Id == filteredQuestionId && x.Property == QuestionnaireVerificationReferenceProperty.OptionsFilter), Is.True);
        }

        [Test]
        public void when_replace_texts_in_option_filter_expression_in_categorical__linked_question()
        {
            string searchFor = "to_search";
            string replaceWith = "replaced";
            Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Guid filteredQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId, groupId: chapterId);
            questionnaire.AddTextListQuestion(Id.g1, chapterId, responsibleId);
            questionnaire.AddMultiOptionQuestion(filteredQuestionId, chapterId, responsibleId,
                linkedToQuestionId: Id.g1, optionsFilterExpression: $"filter with {searchFor}");

            questionnaire.ReplaceTexts(Create.Command.ReplaceTextsCommand(searchFor, replaceWith, userId: responsibleId));

            var question = questionnaire.QuestionnaireDocument.Find<IQuestion>(filteredQuestionId);
            Assert.That(question.Properties.OptionsFilterExpression, Is.EqualTo($"filter with {replaceWith}"));
            Assert.That(questionnaire.GetLastReplacedEntriesCount(), Is.EqualTo(1));
        }
    }
}

#nullable enable
using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(FindReplaceService))]
    public class FindReplaceServiceTests
    {
        private static FindReplaceService CreateService(IDesignerQuestionnaireStorage? storage = null)
            => new FindReplaceService(storage ?? Mock.Of<IDesignerQuestionnaireStorage>());

        [Test]
        public void FindAll_when_questionnaire_is_not_found_should_throw()
        {
            var service = CreateService();

            Assert.Throws<InvalidOperationException>(() =>
                service.FindAll(new QuestionnaireRevision(Guid.NewGuid()), "a", false, false, false).ToList());
        }

        [Test]
        public void FindAll_should_find_question_by_title_ignoring_case_by_default()
        {
            var question = Create.TextQuestion(variable: "q1", text: "Household Size");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            var result = CreateService().FindAll(document, "household", matchCase: false, matchWholeWord: false, useRegex: false).ToList();

            result.Should().ContainSingle(x => x.Id == question.PublicKey);
        }

        [Test]
        public void FindAll_when_match_case_should_not_find_different_case()
        {
            var question = Create.TextQuestion(variable: "q1", text: "Household Size");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            CreateService().FindAll(document, "household", matchCase: true, matchWholeWord: false, useRegex: false)
                .Should().BeEmpty();
        }

        [Test]
        public void FindAll_when_match_whole_word_should_not_find_partial_word()
        {
            var question = Create.TextQuestion(variable: "q1", text: "Households");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            CreateService().FindAll(document, "Household", matchCase: false, matchWholeWord: true, useRegex: false)
                .Should().BeEmpty();
        }

        [Test]
        public void FindAll_when_use_regex_should_apply_pattern()
        {
            var question = Create.TextQuestion(variable: "q1", text: "Item 42");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            CreateService().FindAll(document, @"Item \d+", false, false, useRegex: true)
                .Should().ContainSingle(x => x.Id == question.PublicKey);
        }

        [Test]
        public void FindAll_when_regex_special_chars_and_not_regex_should_treat_as_literal()
        {
            var question = Create.TextQuestion(variable: "q1", text: "a.b");
            var other = Create.TextQuestion(variable: "q2", text: "axb");
            var document = Create.QuestionnaireDocumentWithOneChapter(question, other);

            CreateService().FindAll(document, "a.b", false, false, useRegex: false)
                .Select(x => x.Id).Should().BeEquivalentTo(new[] { question.PublicKey });
        }

        [Test]
        public void FindAll_should_find_entity_by_variable_name()
        {
            var question = Create.TextQuestion(variable: "unique_var", text: "Text");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            var result = CreateService().FindAll(document, "unique_var", false, false, false).ToList();

            result.Should().ContainSingle(x => x.Property == QuestionnaireVerificationReferenceProperty.VariableName);
        }

        [Test]
        public void FindAll_should_find_entity_by_its_id()
        {
            var question = Create.TextQuestion(variable: "q1", text: "Text");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            CreateService().FindAll(document, question.PublicKey.ToString(), false, false, false)
                .Should().ContainSingle(x => x.Id == question.PublicKey);
        }

        [Test]
        public void FindAll_should_find_text_in_enabling_condition()
        {
            var question = Create.TextQuestion(variable: "q1", text: "Text", enablementCondition: "age > 18");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            CreateService().FindAll(document, "age", false, false, false)
                .Should().ContainSingle(x => x.Property == QuestionnaireVerificationReferenceProperty.EnablingCondition);
        }

        [Test]
        public void FindAll_should_find_text_in_macro_content()
        {
            var document = Create.QuestionnaireDocumentWithOneChapter();
            var macroId = Guid.NewGuid();
            document.Macros.Add(macroId, new Macro { Name = "m", Content = "self > 100" });

            var result = CreateService().FindAll(document, "100", false, false, false).ToList();

            result.Should().ContainSingle(x => x.Id == macroId);
        }

        [Test]
        public void ReplaceTexts_when_questionnaire_is_not_found_should_throw()
        {
            var service = CreateService();

            Assert.Throws<InvalidOperationException>(() =>
                service.ReplaceTexts(new QuestionnaireRevision(Guid.NewGuid()), Guid.NewGuid(), "a", "b", false, false, false));
        }

        [Test]
        public void ReplaceTexts_should_replace_title_and_return_number_of_affected_entities()
        {
            var q1 = Create.TextQuestion(variable: "q1", text: "Old name");
            var q2 = Create.TextQuestion(variable: "q2", text: "Another Old");
            var q3 = Create.TextQuestion(variable: "q3", text: "Untouched");
            var document = Create.QuestionnaireDocumentWithOneChapter(q1, q2, q3);

            var affected = CreateService().ReplaceTexts(document, Guid.NewGuid(), "old", "new", false, false, false);

            affected.Should().Be(2);
            q1.QuestionText.Should().Be("new name");
            q2.QuestionText.Should().Be("Another new");
            q3.QuestionText.Should().Be("Untouched");
        }

        [Test]
        public void ReplaceTexts_should_replace_in_condition_and_variable_name()
        {
            var question = Create.TextQuestion(variable: "old_var", text: "Text", enablementCondition: "old_var > 1");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            var affected = CreateService().ReplaceTexts(document, Guid.NewGuid(), "old_var", "new_var", true, false, false);

            affected.Should().Be(1);
            question.StataExportCaption.Should().Be("new_var");
            question.ConditionExpression.Should().Be("new_var > 1");
        }

        [Test]
        public void ReplaceTexts_should_replace_in_macro_content()
        {
            var document = Create.QuestionnaireDocumentWithOneChapter();
            var macro = new Macro { Name = "m", Content = "a + a" };
            document.Macros.Add(Guid.NewGuid(), macro);

            var affected = CreateService().ReplaceTexts(document, Guid.NewGuid(), "a", "b", true, true, false);

            affected.Should().Be(1);
            macro.Content.Should().Be("b + b");
        }

        [Test]
        public void ReplaceTexts_when_nothing_matches_should_return_zero()
        {
            var question = Create.TextQuestion(variable: "q1", text: "Text");
            var document = Create.QuestionnaireDocumentWithOneChapter(question);

            CreateService().ReplaceTexts(document, Guid.NewGuid(), "zzz", "b", false, false, false).Should().Be(0);
        }
    }
}


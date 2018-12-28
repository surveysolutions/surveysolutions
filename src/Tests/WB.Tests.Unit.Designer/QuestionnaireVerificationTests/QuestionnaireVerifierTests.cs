using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class QuestionnaireVerifierTests : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void when_verifying_questionnaire_having_question_instructions_referencing_self_in_substitution()
        {
            Guid questionWithSelfSubstitutionsId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Question(questionId: questionWithSelfSubstitutionsId, variable: "me",
                    instructions: "hello %me%!"),
            });

            var verifier = CreateQuestionnaireVerifier();

            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();
            
            verificationMessages.ShouldContainError("WB0016");

            var error = verificationMessages.GetError("WB0016");
            error.References.Count().Should().Be(1);
            error.References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            error.References.First().Id.Should().Be(questionWithSelfSubstitutionsId);
        }

        [Test]
        public void when_verifying_questionnaire_having_question_instructions_referencing_non_existing_item_in_substitution()
        {
            Guid questionWithSelfSubstitutionsId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Question(questionId: questionWithSelfSubstitutionsId, variable: "me",
                    instructions: "hello %unknown%!"),
            });

            var verifier = CreateQuestionnaireVerifier();

            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

            verificationMessages.ShouldContainError("WB0017");

            var error = verificationMessages.GetError("WB0017");
            error.References.Count().Should().Be(1);
            error.References.First().Id.Should().Be(questionWithSelfSubstitutionsId);
        }

        [Test]
        public void when_verifying_questionnaire_having_question_instructions_referencing_dipper_scope_in_substitution()
        {
            Guid rosterId = Guid.Parse("13333333333333333333333333333333");
            Guid questionId = Guid.Parse("10000000000000000000000000000000");
            Guid variableId = Guid.Parse("12222222222222222222222222222222");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
                {
                    Create.TextQuestion(questionId, instruction: "Var: %variable1%"),
                    Create.FixedRoster(rosterId, fixedTitles: new[] {"1", "2", "3"}, children: new List<IComposite>
                    {
                        Create.Variable(variableId, VariableType.LongInteger, "variable1")
                    })
                }));

             var verifier = CreateQuestionnaireVerifier();
             var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0019");

            verificationMessages.GetError("WB0019")
                    .References.Select(x => x.Type)
                    .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Question, QuestionnaireVerificationReferenceType.Variable);

            verificationMessages.GetError("WB0019").References.ElementAt(0).Id.Should().Be(questionId);
            verificationMessages.GetError("WB0019").References.ElementAt(1).Id.Should().Be(variableId);
        }

        [Test]
        public void when_verifying_questionnaire_having_question_referencing_parent_group_in_conditions()
        {
            Guid groupId = Guid.Parse("13333333333333333333333333333333");
            Guid numericQuestionId = Guid.Parse("10000000000000000000000000000000");
            

            string groupVariable = "group1";

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.Group(groupId, "Group X", groupVariable, null, false, new IComposite[]
                {
                    Create.NumericIntegerQuestion(numericQuestionId,
                        variable:"num", enablementCondition:$"IsSectionAnswered({groupVariable})")
                })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            AQ: verificationMessages.ShouldContainError("WB0056");

            verificationMessages.GetError("WB0056")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.GetError("WB0056").References.ElementAt(0).Id.Should().Be(numericQuestionId);
        }
        [Test]
        public void when_verifying_question_text_with_markdown_links_to_unknown_question_then_should_return_WB0278_errors()
        {
            Guid questionId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId, text: "[link to unknown entity](unknownVar)")
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0278");

            verificationMessages.GetError("WB0278")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.GetError("WB0278").References.ElementAt(0).Id.Should().Be(questionId);
        }

        [Test]
        public void when_verifying_static_text_with_markdown_links_to_unknown_question_then_should_return_WB0278_errors()
        {
            Guid staticTextId = Guid.Parse("20000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.StaticText(staticTextId, text: "[link to unknown entity](unknownVar)"),
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0278");

            verificationMessages.GetError("WB0278")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.StaticText);

            verificationMessages.GetError("WB0278").References.ElementAt(0).Id.Should().Be(staticTextId);
        }

        [Test]
        public void when_verifying_question_with_markdown_link_to_unknown_question_in_validation_message()
        {

            Guid questionId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId, validationConditions: new []{Create.ValidationCondition(message: "[link to unknown entity](unknownVar)") })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0278");

            verificationMessages.GetError("WB0278")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.GetError("WB0278")
                .References.Select(x => x.IndexOfEntityInProperty)
                .Should().BeEquivalentTo(0);

            verificationMessages.GetError("WB0278").References.ElementAt(0).Id.Should().Be(questionId);
        }

        [Test]
        public void when_verifying_static_text_with_markdown_link_to_unknown_question_in_validation_message()
        {

            Guid staticTextId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.StaticText(staticTextId, validationConditions: new []{Create.ValidationCondition(message: "[link to unknown entity](unknownVar)") })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0278");

            verificationMessages.GetError("WB0278")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.StaticText);

            verificationMessages.GetError("WB0278")
                .References.Select(x => x.IndexOfEntityInProperty)
                .Should().BeEquivalentTo(0);

            verificationMessages.GetError("WB0278").References.ElementAt(0).Id.Should().Be(staticTextId);
        }

        [TestCase("cover")]
        [TestCase("complete")]
        [TestCase("overview")]
        public void when_verifying_static_text_with_markdown_link_to_system_variable_in_validation_message(string systemVariable)
        {

            Guid staticTextId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.StaticText(staticTextId, validationConditions: new []{Create.ValidationCondition(message: $"[link to unknown entity]({systemVariable})") })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldNotContainError("WB0278");
        }
        [TestCase("cover")]
        [TestCase("complete")]
        [TestCase("overview")]
        public void when_verifying_question_with_markdown_link_to_unknown_question_in_validation_message(string systemVariable)
        {

            Guid questionId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId, validationConditions: new []{Create.ValidationCondition(message: $"[link to unknown entity]({systemVariable})") })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldNotContainError("WB0278");
        }
    }
}

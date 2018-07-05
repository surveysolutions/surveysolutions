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
    }
}

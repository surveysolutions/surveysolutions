using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    internal class QuestionnaireVerifierTests : QuestionnaireVerifierTestsContext
    {
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

            verificationMessages.ShouldContainError("WB0056");

            verificationMessages.GetError("WB0056")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.GetError("WB0056").References.ElementAt(0).Id.Should().Be(numericQuestionId);
        }
        [Test]
        public void when_verifying_question_text_with_markdown_links_to_unknown_question_then_should_return_WB0280_errors()
        {
            Guid questionId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId, text: "[link to unknown entity](unknownVar)")
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0280");

            verificationMessages.GetError("WB0280")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.GetError("WB0280").References.ElementAt(0).Id.Should().Be(questionId);
        }

        [Test]
        public void when_verifying_static_text_with_markdown_links_to_unknown_question_then_should_return_WB0280_errors()
        {
            Guid staticTextId = Guid.Parse("20000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.StaticText(staticTextId, text: "[link to unknown entity](unknownVar)"),
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0280");

            verificationMessages.GetError("WB0280")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.StaticText);

            verificationMessages.GetError("WB0280").References.ElementAt(0).Id.Should().Be(staticTextId);
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


            verificationMessages.ShouldContainError("WB0280");

            verificationMessages.GetError("WB0280")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Question);

            verificationMessages.GetError("WB0280")
                .References.Select(x => x.IndexOfEntityInProperty)
                .Should().BeEquivalentTo(0);

            verificationMessages.GetError("WB0280").References.ElementAt(0).Id.Should().Be(questionId);
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


            verificationMessages.ShouldContainError("WB0280");

            verificationMessages.GetError("WB0280")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.StaticText);

            verificationMessages.GetError("WB0280")
                .References.Select(x => x.IndexOfEntityInProperty)
                .Should().BeEquivalentTo(0);

            verificationMessages.GetError("WB0280").References.ElementAt(0).Id.Should().Be(staticTextId);
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


            verificationMessages.ShouldNotContainError("WB0280");
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


            verificationMessages.ShouldNotContainError("WB0280");
        }

        [Test]
        public void when_verifying_entities_count_on_nested_fixed_rosters()
        {
            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.FixedRoster(fixedTitles: Enumerable.Range(1, 60).Select(i => i.ToString()), children: new IComposite[]
                {
                    Create.FixedRoster(fixedTitles: Enumerable.Range(1, 60).Select(i => i.ToString()), children: new IComposite[]
                    {
                        Create.FixedRoster(fixedTitles: Enumerable.Range(1, 60).Select(i => i.ToString()), children: new IComposite[]
                        {
                            Create.TextQuestion()
                        })
                    })
                })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

            verificationMessages.ShouldContainError("WB0281");
        }

        [Test]
        public void when_verifying_entities_count_on_nested_fixed_rosters_that_do_not_exceed_limit()
        {
            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.FixedRoster(fixedTitles: Enumerable.Range(1, 2).Select(i => i.ToString()), children: new IComposite[]
                {
                    Create.TextQuestion(),
                    Create.FixedRoster(fixedTitles: Enumerable.Range(1, 60).Select(i => i.ToString()), children: new IComposite[]
                    {
                        Create.TextQuestion(),
                        Create.FixedRoster(fixedTitles: Enumerable.Range(1, 60).Select(i => i.ToString()), children: new IComposite[]
                        {
                            Create.TextQuestion()
                        })
                    }),
                    Create.FixedRoster(fixedTitles: Enumerable.Range(1, 60).Select(i => i.ToString()), children: new IComposite[]
                    {
                        Create.TextQuestion(),
                        Create.FixedRoster(fixedTitles: Enumerable.Range(1, 60).Select(i => i.ToString()), children: new IComposite[]
                        {
                            Create.TextQuestion()
                        })
                    })
                })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

            verificationMessages.ShouldNotContainError("WB0281");
        }

        [Test]
        public void when_verifying_questionnaire_that_has_categories_with_not_unique_name()
        {
            // arrange
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var categoriesId = Guid.Parse("11111111111111111111111111111111");

            var questionnaire = Create.QuestionnaireDocument("qVar", children: new IComposite[]
            {
                Create.TextQuestion(questionId, variable: "q")
            }, categories: new[] {Create.Categories(categoriesId, "q")});

            var verifier = CreateQuestionnaireVerifier();

            // act
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            // assert
            verificationMessages.ShouldContainCritical("WB0026");
            verificationMessages.GetCritical("WB0026").MessageLevel.Should().Be(VerificationMessageLevel.Critical);
            verificationMessages.GetCritical("WB0026").References.Count().Should().Be(2);
            verificationMessages.GetCritical("WB0026").References.First().Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            verificationMessages.GetCritical("WB0026").References.First().Id.Should().Be(questionId);
            verificationMessages.GetCritical("WB0026").References.Last().Type.Should().Be(QuestionnaireVerificationReferenceType.Categories);
            verificationMessages.GetCritical("WB0026").References.Last().Id.Should().Be(categoriesId);
        }
        
        [Test]
        public void when_verifying_questionnaire_having_table_roster_with_substitution_to_a_child_question()
        {
            Guid rosterId = Guid.Parse("13333333333333333333333333333333");
            Guid questionId = Guid.Parse("10000000000000000000000000000000");
            Guid variableId = Guid.Parse("12222222222222222222222222222222");

            var questionnaire = Create.QuestionnaireDocument(children: Create.Chapter(children: new IComposite[]
            {
                Create.FixedRoster(rosterId, title:"roster title %name%", displayMode:RosterDisplayMode.Table,fixedTitles: new[] {"1", "2", "3"}, children: new List<IComposite>
                {
                    Create.TextQuestion(questionId, variable:"name")
                })
            }));

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();


            verificationMessages.ShouldContainError("WB0313");

            verificationMessages.GetError("WB0313")
                .References.Select(x => x.Type)
                .Should().BeEquivalentTo(QuestionnaireVerificationReferenceType.Group, QuestionnaireVerificationReferenceType.Question);

            verificationMessages.GetError("WB0313").References.ElementAt(0).Id.Should().Be(rosterId);
            verificationMessages.GetError("WB0313").References.ElementAt(1).Id.Should().Be(questionId);
        }
    }
}

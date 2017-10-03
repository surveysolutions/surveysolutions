using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(SubstitutionText))]
    [TestFixture]
    public class SubstitutionTextTests
    {
        [Test]
        public void When_ReplaceSubstitutions_for_element_with_referancec_on_parent_rosters_Then_should_return_text_with_roster_titles()
        {
            //arrange
            var rosterId1 = Guid.Parse("22222222222222222222222222222222");
            var rosterId2 = Guid.Parse("33333333333333333333333333333333");
            var questionId = Guid.Parse("44444444444444444444444444444444");

            var questionnireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Roster(rosterId1, variable: "r1", children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId2, variable: "r2", children: new IComposite[]
                    {
                        Create.Entity.NumericQuestion(questionId)
                    })
                })
            });
            var questionnire = Create.Entity.PlainQuestionnaire(questionnireDocument);

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeRoster(Create.Entity.Identity(rosterId1, new decimal[] { 2 }), rosterTitle: "title 2", children: new IInterviewTreeNode[]
                {
                    Create.Entity.InterviewTreeRoster(Create.Entity.Identity(rosterId2, new decimal[] { 2, 1}), rosterTitle: "title 2.1", children: new IInterviewTreeNode[]
                    {
                        Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(questionId), questionType: QuestionType.Numeric, answer: 5),
                    }),
                }),
            });
            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection);


            var substitionTextFactory = Create.Service.SubstitutionTextFactory();
            var questionIdentity = Create.Entity.Identity(questionId, new RosterVector(new decimal[] { 2, 1 }));
            var substitionText = substitionTextFactory.CreateText(questionIdentity, "title: %r1% %r2%", questionnire);
            substitionText.SetTree(tree);

            //act
            substitionText.ReplaceSubstitutions();

            //assert
            Assert.That(substitionText.HasSubstitutions, Is.True);
            Assert.That(substitionText.Text, Is.EqualTo("title: title 2 title 2.1"));
        }

        [Test]
        public void when_substitution_question_contains_non_existing_question_should_not_fail()
        {
            var targetEntityId = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), Create.Entity.RosterVector(1));

            var substitutedVariableId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            SubstitutionText text = CreateSubstitutionText(targetEntityId, "%rostertitle% %question% %variable%",
                new SubstitutionVariable
                {
                    Id = substitutedVariableId,
                    Name = "question"
                }
            );

            var variableRawValue = "<b>variable value</b>";

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeVariable(Create.Entity.Identity(substitutedVariableId), value: variableRawValue)
            });

            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection);

            text.SetTree(tree);

            Assert.DoesNotThrow(() => text.ReplaceSubstitutions(), "KP-10048 hrvs2: error on interview details");
        }

        [Test]
        public void when_replacing_text_with_answer_containing_html_Should_html_encode_answer()
        {
            var targetEntityId = Create.Entity.Identity(Guid.Parse("11111111111111111111111111111111"), Create.Entity.RosterVector(1));

            var substitutedQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariableId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var substitutedRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            SubstitutionText text = CreateSubstitutionText(targetEntityId,
                "%rostertitle% %question% %variable%",
                new SubstitutionVariable
                {
                    Id = substitutedQuestionId,
                    Name = "question"
                },
                new SubstitutionVariable
                {
                    Id = substitutedRosterId,
                    Name = "rostertitle"
                },
                new SubstitutionVariable
                {
                    Id = substitutedVariableId,
                    Name = "variable"
                }
            );

            var questionRawAnswer = "<b>question answer</b>";
            var variableRawValue = "<b>variable value</b>";
            var rosterRawValue = "<b>roster</b>";

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeRoster(Create.Entity.Identity(substitutedRosterId, new decimal[] {1}),
                    rosterTitle: rosterRawValue,
                    children: Create.Entity.InterviewTreeStaticText(targetEntityId)),
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(substitutedQuestionId), answer: questionRawAnswer),
                Create.Entity.InterviewTreeVariable(Create.Entity.Identity(substitutedVariableId), value: variableRawValue)
            });

            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection);
            text.SetTree(tree);

            text.ReplaceSubstitutions();
            var browserReadyText = text.BrowserReadyText;

            Assert.That(browserReadyText, Is.Not.Null.Or.Empty);

            Assert.That(browserReadyText, Does.Contain(HttpUtility.HtmlEncode(questionRawAnswer)));
            Assert.That(browserReadyText, Does.Contain(HttpUtility.HtmlEncode(variableRawValue)));
            Assert.That(browserReadyText, Does.Contain(HttpUtility.HtmlEncode(rosterRawValue)));

            Assert.That(browserReadyText, Does.Not.Contain("<"));
            Assert.That(browserReadyText, Does.Not.Contain(">"));

            var substitutedRawText = text.Text;
            Assert.That(substitutedRawText, Is.EqualTo($"{rosterRawValue} {questionRawAnswer} {variableRawValue}"));
        }

        private SubstitutionText CreateSubstitutionText(
            Identity id,
            string template,
            params SubstitutionVariable[] variables)
        {
            SubstitutionText text = new SubstitutionText(id,
                template,
                variables.ToList(),
                Create.Service.SubstitutionService(),
                Create.Service.VariableToUIStringService());

            return text;
        }
    }
}
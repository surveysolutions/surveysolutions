using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
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
        public void When_ReplaceSubstitutions_for_element_with_reference_on_parent_rosters_Then_should_return_text_with_roster_titles()
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


            //act
            substitionText.ReplaceSubstitutions(tree);

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
                null,
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

            Assert.DoesNotThrow(() => text.ReplaceSubstitutions(tree), "KP-10048 hrvs2: error on interview details");
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
                null,
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

            text.ReplaceSubstitutions(tree);
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

        [Test]
        public void when_date_and_dateTime_question_is_used_in_substitutions()
        {
            var substitutedQuestionId1 = Id.g1;
            var substitutedQuestionId2 = Id.g2;

            SubstitutionText text = CreateSubstitutionText(Create.Identity(Id.gA),
                "%date% %dateTime%",
                null,
                new SubstitutionVariable
                {
                    Id = substitutedQuestionId1,
                    Name = "date"
                },
                new SubstitutionVariable
                {
                    Id = substitutedQuestionId2,
                    Name = "dateTime"
                }
            );
            var dateAnswer = new DateTime(2010, 4, 6);
            var dateTimeAnswer = new DateTime(2010, 4, 6, 4, 30, 50);

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(substitutedQuestionId1), answer: dateAnswer, questionType: QuestionType.DateTime),
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(substitutedQuestionId2), answer: dateTimeAnswer, questionType: QuestionType.DateTime, isTimestamp: true)
            });

            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection);

            // Act
            text.ReplaceSubstitutions(tree);
            var browserReadyText = text.BrowserReadyText;

            Assert.That(browserReadyText, Is.EqualTo($"<time date=\"2010-04-06\">2010-04-06</time> <time datetime=\"2010-04-06T04:30:50.0000000\">2010-04-06 04:30:50</time>"));
        }
        
        [Test]
        public void when_numeric_question_with_special_value_is_used_in_substitutions()
        {
            var questionId = Guid.Parse("44444444444444444444444444444444");

            var substitutedQuestionId1 = Id.g1;
            var substitutedQuestionId2 = Id.g2;

            var questionnireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.NumericQuestion(substitutedQuestionId1, variableName:"numeric1"),
                Create.Entity.NumericQuestion(substitutedQuestionId2, variableName:"numeric2",options: new List<Answer>(){new Answer(){AnswerCode = 6, AnswerText = "test"}}),
                Create.Entity.NumericQuestion(questionId, title:"%numeric1% %numeric2%")
            });

            var questionnire = Create.Entity.PlainQuestionnaire(questionnireDocument);

            var numericAnswer1 = 5;
            var numericAnswer2 = 6;

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(substitutedQuestionId1), answer: numericAnswer1, questionType: QuestionType.Numeric),
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(substitutedQuestionId2), answer: numericAnswer2, questionType: QuestionType.Numeric),
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(questionId), questionType:QuestionType.Numeric)
            });

            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection, questionnaire:questionnire);

            var substitionTextFactory = Create.Service.SubstitutionTextFactory();
            var questionIdentity = Create.Entity.Identity(questionId, RosterVector.Empty);
            var substitionText = substitionTextFactory.CreateText(questionIdentity, "title: %numeric1% %numeric2%", questionnire);

            // Act
            substitionText.ReplaceSubstitutions(tree);
            
            Assert.That(substitionText.Text, Is.EqualTo("title: 5 test"));
        }

        [Test]
        public void when_substituting_disabled_question_Should_substitute_as_unanswered()
        {
            var substitutedQuestionId1 = Id.g1;

            SubstitutionText text = CreateSubstitutionText(Create.Identity(Id.gA),
                "%subst%",
                null,
                new SubstitutionVariable
                {
                    Id = substitutedQuestionId1,
                    Name = "subst"
                }
            );

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(substitutedQuestionId1), 
                    answer: "answer", 
                    questionType: QuestionType.Text,
                    isDisabled: true)
            });

            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection);

            // Act
            text.ReplaceSubstitutions(tree);
            var browserReadyText = text.BrowserReadyText;

            Assert.That(browserReadyText, Is.EqualTo($"[...]"));
        }

        [Test]
        public void when_create_substitution_text_with_markdown_syntax_then_any_exceptions_should_not_be_throwing()
        {
            TestDelegate testDelegate = () =>
                CreateSubstitutionText(Id.Identity1,
                    "2. Mira este dibujo aquí arriba (SEÑALE ARRIBA). \n" +
                    "<br>A este dibujo le falta un tuquito. \n\n<br>" +
                    "Aquí abajo tenemos 6 tuquitos diferentes (SEÑALE ABAJO)\n" +
                    "<br>Solo uno de estos tuquitos es el que completa a este dibujo.");
            
            Assert.That(testDelegate,
                Throws.Nothing);
        }

        [Test]
        public void should_substitute_self_value()
        {
            var substitutedQuestionId1 = Id.g1;

            SubstitutionText text = CreateSubstitutionText(Create.Identity(Id.gA),
                "%self%",
                "subst",
                new SubstitutionVariable
                {
                    Id = substitutedQuestionId1,
                    Name = "subst"
                }
            );

            var sourceTreeMainSection = Create.Entity.InterviewTreeSection(children: new IInterviewTreeNode[]
            {
                Create.Entity.InterviewTreeQuestion(Create.Entity.Identity(substitutedQuestionId1),
                    answer: "answer")
            });

            var tree = Create.Entity.InterviewTree(sections: sourceTreeMainSection);

            // Act
            text.ReplaceSubstitutions(tree);
            var browserReadyText = text.BrowserReadyText;

            Assert.That(browserReadyText, Is.EqualTo($"answer"));

        }

        private SubstitutionText CreateSubstitutionText(
            Identity id,
            string template,
            string selfVariableName = "self",
            params SubstitutionVariable[] variables)
        {
            SubstitutionText text = new SubstitutionText(id,
                QuestionnaireMarkdown.ToHtml(template),
                selfVariableName,
                variables.ToList(),
                Create.Service.SubstitutionService(),
                Create.Service.VariableToUIStringService());

            return text;
        }

        [Test]
        public void when_create_substitution_text_with_markdown_lists_then_lists_markup_should_not_be_converted_into_html()
        {
            // arrange
            var markdownTextWithListItem = "1. List option 1";

            // act
            var substitutionText = CreateSubstitutionText(Id.Identity1, markdownTextWithListItem);

            // assert
            Assert.That(substitutionText.Text, Is.EqualTo(markdownTextWithListItem));
        }

        [Test]
        public void when_create_substitution_text_with_spec_symbols_then_it_symbols_should_be_encoded_but_not_removed()
        {
            // arrange
            var specSymbolsForMarkdownEngine = "<>&\"";
            var markdownText = "0.1,2/3'4;5|6]7[8=9-012?3}4{5+6_7)8(9*01^2%3$4#5@6!7~8`";

            // act
            var substitutionText = CreateSubstitutionText(Id.Identity1, markdownText + specSymbolsForMarkdownEngine);

            // assert
            Assert.That(substitutionText.Text, Is.EqualTo(markdownText + HttpUtility.HtmlEncode(specSymbolsForMarkdownEngine)));
        }
    }
}

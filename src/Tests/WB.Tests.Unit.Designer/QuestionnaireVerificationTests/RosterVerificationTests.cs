using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    [TestFixture]
    internal class RosterVerificationTests : QuestionnaireVerifierTestsContext
    {

        [Test]
        public void when_roster_title_has_markdown_link()
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(Id.g1, variable: "q1"),
                Create.NumericRoster(rosterId: Id.gA, title: "Roster [move to q1](q1)", rosterSizeQuestionId: Id.g1, rosterTitleQuestionId:Id.g2, variable: "r1", children: new IComposite[]
                {
                    Create.TextQuestion(Id.g2, variable: "tb")
                })
            });

            var verifier = CreateQuestionnaireVerifier();

            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            verificationMessages.ShouldContainError("WB0057");
            Assert.That(verificationMessages.GetError("WB0057").References.First().Id, Is.EqualTo(Id.gA));
        }

    
        [Test]
        public void when_exists_roster_with_roster_title_question_then_circular_reference_should_not_be_exists()
        {
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(intQuestionId, variable: "i1"),
                Create.NumericRoster(rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId:rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                {
                    Create.TextQuestion(rosterTitleQuestionId, variable: "tb")
                })
            });

            var verifier = CreateQuestionnaireVerifier();
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

            verificationMessages.ShouldNotContainError("WB0056");
            verificationMessages.GetError("WB0056").Should().BeNull();
        }

        [Test]
        public void should_validate_location_of_roster_title()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.FixedRoster(title: "Roster %rostertitle%",
                    rosterId: Id.gA,
                    children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(variable: "test1", id: Id.g1)
                    }
                ))
                .ExpectError("WB0059");
        }

        [Test]
        public void should_not_allow_roster_title_inside_matrix_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.FixedRoster(title: "Roster ",
                        rosterId: Id.gA,
                        displayMode:RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.SingleQuestion(variable: "test1 %rostertitle%", id:Id.g1)
                        }
                    ))
                .ExpectError("WB0300");
        }

        [Test]
        public void should_not_allow_roster_title_inside_matrix_roster_triggered_by_numeric()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(Id.g5),
                    Create.Roster(
                        rosterSizeQuestionId: Id.g5,
                        title: "Roster ",
                        rosterId: Id.gA,
                        displayMode: RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.SingleQuestion(variable: "test1 %rostertitle%", id:Id.g1)
                        }
                    ))
                .ExpectError("WB0300");
        }

        [Test]
        public void should_validate_location_of_roster_title_for_numeric_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(id: Id.g1),
                Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, title: "title %rostertitle%")
            )
            .ExpectError("WB0059");
        }

        [Test]
        public void should_allow_only_10_ui_elements_in_plain_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(id: Id.g1),
                Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Flat, 
                    children: new IComposite[]
                    {
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.StaticText(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.Question(),
                        Create.StaticText(),
                    })
            ) 
            .ExpectError("WB0278");
        }
        
        [Test]
        public void should_allow_more_than_10_non_ui_elements_in_plain_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Flat, 
                        children: new IComposite[]
                        {
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.StaticText(),
                            Create.Variable(),
                            Create.Variable(),
                            
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.StaticText(),
                            Create.Variable(),
                            Create.Variable(),
                        })
                ) 
                .ExpectNoError("WB0278");
        }

        [Test]
        public void should_reject_nested_roster_in_plain_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(id: Id.g1),
                Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Flat, 
                    children: new IComposite[]
                    {
                        Create.Question(),
                        Create.Roster(),
                    })
            ) 
            .ExpectError("WB0279");
        }


        [Test]
        public void should_doesnt_allow_nested_groups_in_table_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Table,
                        children: new IComposite[]
                        {
                            Create.Group(),
                        })
                )
                .ExpectError("WB0282");
        }

        [Test]
        public void should_allow_only_10_questions_in_table_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Table,
                        children: new IComposite[]
                        {
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                            Create.Question(),
                        })
                )
                .ExpectError("WB0283");
        }

        [Test]
        public void should_not_allow_supervisor_questions_in_table_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Table,
                        children: new IComposite[]
                        {
                            Create.Question(scope: QuestionScope.Supervisor),
                        })
                )
                .ExpectError("WB0284");
        }

        [TestCase(QuestionType.Area)]
        [TestCase(QuestionType.Audio)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.GpsCoordinates)]
        [TestCase(QuestionType.Multimedia)]
        [TestCase(QuestionType.MultyOption)]
        [TestCase(QuestionType.QRBarcode)]
        [TestCase(QuestionType.SingleOption)]
        [TestCase(QuestionType.TextList)]
        public void should_allow_only_text_and_numeric_questions_in_table_roster(QuestionType questionType)
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Table,
                        children: new IComposite[]
                        {
                            Create.Question(questionType: questionType),
                        })
                )
                .ExpectError("WB0285");
        }

        [Test]
        public void should_not_allow_static_text_in_table_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Table,
                        children: new IComposite[]
                        {
                            Create.StaticText(),
                        })
                )
                .ExpectError("WB0285");
        }

        [Test]
        public void should_show_warning_on_table_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Table,
                        children: new IComposite[]
                        {
                            Create.Question(),
                        })
                )
                .ExpectWarning("WB0286");
        }

        [TestCase("title %i1%")]
        [TestCase("title %self%")]
        [TestCase("title %q1%")]
        public void should_show_error_on_question_with_substitution_in_title_inside_table_roster(string title)
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1, variable: "i1"),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Table,
                        children: new IComposite[]
                        {
                            Create.Question(title: title, variable: "q1"),
                        })
                )
                .ExpectError("WB0287");
        }

        [TestCase(QuestionType.Area)]
        [TestCase(QuestionType.Audio)]
        [TestCase(QuestionType.DateTime)]
        [TestCase(QuestionType.GpsCoordinates)]
        [TestCase(QuestionType.Multimedia)]
        [TestCase(QuestionType.Numeric)]
        [TestCase(QuestionType.QRBarcode)]
        [TestCase(QuestionType.Text)]
        [TestCase(QuestionType.TextList)]
        public void should_allow_only_categorical_questions_in_matrix_roster(QuestionType questionType)
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.Question(questionType: questionType),
                        })
                )
                .ExpectError("WB0297");
        }

        [Test]
        public void should_show_warning_on_matrix_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.Question(),
                        })
                )
                .ExpectWarning("WB0286");
        }

        [Test]
        public void should_not_allow_supervisor_questions_in_matrix_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.Question(scope: QuestionScope.Supervisor),
                        })
                )
                .ExpectError("WB0299");
        }

        [Test]
        public void should_not_allow_linked_to_question_questions_in_matrix_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.Question(linkedToQuestion: Id.g7),
                        })
                )
                .ExpectError("WB0301");
        }

        [Test]
        public void should_not_allow_linked_to_roster_questions_in_matrix_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.Question(linkedToRoster: Id.g7),
                        })
                )
                .ExpectError("WB0301");
        }

        [Test]
        public void should_not_allow_static_text_in_matrix_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(id: Id.g1),
                Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Matrix,
                    children: new IComposite[]
                    {
                        Create.StaticText()
                    })
                )
                .ExpectError("WB0297");
        }

        [Test]
        public void should_allow_only_1_questions_in_Matrix_roster()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericIntegerQuestion(id: Id.g1),
                    Create.NumericRoster(rosterId: Id.g2, rosterSizeQuestionId: Id.g1, displayMode: RosterDisplayMode.Matrix,
                        children: new IComposite[]
                        {
                            Create.Question(),
                            Create.Question()
                        })
                )
                .ExpectError("WB0298");
        }

        [Test]
        public void should_not_allow_matrix_roster_to_have_titletitle_variable()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.Roster(displayMode: RosterDisplayMode.Matrix,
                        title: "title - %rostertitle% - end")
                )
                .ExpectError("WB0303");
        }
        
        [Test]
        public void should_allow_matrix_roster_to_have_custom_title()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.Roster(displayMode: RosterDisplayMode.Matrix,
                        customRosterTitle: true)
                )
                .ExpectNoError("WB0303");
        }

        [Test]
        public void should_not_allow_table_roster_to_have_titletitle_variable()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.Roster(displayMode: RosterDisplayMode.Table,
                        title: "title - %rostertitle% - end")
                )
                .ExpectError("WB0304");
        }

        [Test]
        public void should_allow_table_roster_to_have_custom_title()
        {
            Create.QuestionnaireDocumentWithOneChapter(
                    Create.Roster(displayMode: RosterDisplayMode.Table,
                        customRosterTitle: true)
                )
                .ExpectNoError("WB0304");
        }
    }
}

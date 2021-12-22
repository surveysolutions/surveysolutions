using System.Linq;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    [TestOf(typeof(PlainQuestionnaire))]
    internal class PlainQuestionnaireTests : PlainQuestionnaireTestsContext
    {
        [Test]
        public void when_cascading_has_threshold_set_by_user()
        {
            // arrange
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(questionId: Id.g1, answerCodes: new decimal[] {1, 2},
                    variable: "q1"),
                Create.Entity.SingleOptionQuestion(questionId: Id.g2, cascadeFromQuestionId: Id.g1,
                    answerCodes: new decimal[] {1, 2, 3, 4}, parentCodes: new decimal[] {1, 1, 2, 2}, variable: "q2",
                    showAsListThreshold: 3));


            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire, 0);

            // act
            var cascadingAsListThreshold = plainQuestionnaire.GetCascadingAsListThreshold(Id.g2);

            //assert 
            Assert.That(cascadingAsListThreshold, Is.EqualTo(3));
        }

        [Test]
        public void when_cascading_question_can_be_shown_as_list()
        {
            // arrange
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleOptionQuestion(questionId: Id.g1, answerCodes: new decimal[] {1, 2},
                    variable: "q1"),
                Create.Entity.SingleOptionQuestion(questionId: Id.g2, cascadeFromQuestionId: Id.g1,
                    answerCodes: new decimal[] {1, 2, 3, 4}, parentCodes: new decimal[] {1, 1, 2, 2}, variable: "q2",
                    showAsListThreshold: 3));


            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire, 0);

            // act
            var canCascadingBeShownAsList = plainQuestionnaire.ShowCascadingAsList(Id.g2);

            //assert 
            Assert.That(canCascadingBeShownAsList, Is.True);
        }

        [Test]
        public void when_getting_roster_id_by_variable_name_and_variable_name_is_null_should_return_null()
        {
            // arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Group(variable: "group1"),
                Create.Entity.Group(variable: "group2")
            });
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);

            // act
            var rosterId = plainQuestionnaire.GetRosterIdByVariableName(null);

            //assert 
            Assert.That(rosterId, Is.Null);
        }

        [Test]
        public void when_questionnaire_is_configured_to_hide_disabled_members__Should_hide()
        {
            // arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Group(groupId: Id.gA),
                Create.Entity.TextQuestion(questionId: Id.gB),
                Create.Entity.StaticText(publicKey: Id.gC)
            });
            questionnaireDocument.HideIfDisabled = true;

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);

            // act
            var groupIsHidden = plainQuestionnaire.ShouldBeHiddenIfDisabled(Id.gA);
            var questionIsHidden = plainQuestionnaire.ShouldBeHiddenIfDisabled(Id.gB);
            var staticTextHidden = plainQuestionnaire.ShouldBeHiddenIfDisabled(Id.gC);

            //assert 
            Assert.That(groupIsHidden);
            Assert.That(questionIsHidden);
            Assert.That(staticTextHidden);
        }

        [Test]
        public void HasAnyMultimediaQuestion_should_return_true_for_multimedia_question()
        {
            var document = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.MultimediaQuestion());

            // Act
            var hasAnyMultimediaQuestion = Create.Entity.PlainQuestionnaire(document).HasAnyMultimediaQuestion();

            // Assert
            Assert.That(hasAnyMultimediaQuestion);
        }

        [Test]
        public void HasAnyMultimediaQuestion_should_return_true_for_audio_question()
        {
            var document = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.AudioQuestion(Id.gA, "ba1"));

            // Act
            var hasAnyMultimediaQuestion = Create.Entity.PlainQuestionnaire(document).HasAnyMultimediaQuestion();

            // Assert
            Assert.That(hasAnyMultimediaQuestion);
        }

        [Test]
        public void when_getting_static_texts_affected_by_assignments_Should_return_list()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.Variable(Id.g1, VariableType.String, "v1"),
                    Create.Entity.StaticText(Id.g3, attachmentName: "v1")
                }
            );

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);
            var staticTextsThatUseVariableAsAttachment = plainQuestionnaire.GetStaticTextsThatUseVariableAsAttachment(Id.g1);

            Assert.That(staticTextsThatUseVariableAsAttachment, Is.EquivalentTo(new []{Id.g3}));
        }
        
        [Test]
        public void when_identifyed_entities_should_return_questions_static_texts_and_variables()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.g1),
                    Create.Entity.Variable(Id.g2, VariableType.String),
                    Create.Entity.StaticText(Id.g3)
                }
            );
            questionnaire.CoverPageSectionId = questionnaire.Children.First().PublicKey;

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);
            var entities = plainQuestionnaire.GetPrefilledEntities();

            Assert.That(entities.Count, Is.EqualTo(3));
            Assert.That(entities, Is.EquivalentTo(new []{Id.g1, Id.g2, Id.g3}));
        }
    }
}

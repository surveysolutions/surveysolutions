using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefullInterviewTests
    {
        EventContext eventContext;

        public void SetupEventContext()
        {
            eventContext = new EventContext();
        }

        [TearDown]
        public void CleanTests()
        {
            eventContext?.Dispose();
            eventContext = null;
        }

        [Test]
        public void When_question_group_and_roster_have_substitutions_Should_empty_substitutions_replaced_to_ellipsis_with_dots()
        {
            //arrange
            var questionWithSubstitutionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var groupWithSubstitutionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterWithSubsititutionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(variable: "sourceOfSubstitution"),
                Create.Entity.TextQuestion(questionWithSubstitutionId, text: "question with %sourceOfSubstitution%"),
                Create.Entity.Group(groupWithSubstitutionId, title: "group with %sourceOfSubstitution%"),
                Create.Entity.FixedRoster(rosterWithSubsititutionId, title: "roster with %sourceOfSubstitution%",
                    fixedTitles: new[] {Create.Entity.FixedTitle(1, "roster title")}));

            //act
            var interview = Setup.StatefulInterview(questionnaire);

            //assert
            Assert.That(interview.GetTitleText(Identity.Create(questionWithSubstitutionId, RosterVector.Empty)), Is.EqualTo("question with [...]"));
            Assert.That(interview.GetTitleText(Identity.Create(groupWithSubstitutionId, RosterVector.Empty)), Is.EqualTo("group with [...]"));
            Assert.That(interview.GetTitleText(Identity.Create(rosterWithSubsititutionId, Create.Entity.RosterVector(1))), Is.EqualTo("roster with [...]"));
        }

        [Test]
        public void When_create_interview_with_identifier_text_question_Should_apply_answer()
        {
            //arrange
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var question = Create.Entity.TextQuestion(questionId: questionId, variable: "text", preFilled: true);
            var answer = TextAnswer.FromString("value");
            When_create_interview_with_identifier_data_Should_apply_answer(question, answer, "value");
        }

        [Test]
        public void When_create_interview_with_identifier_numeric_question_Should_apply_answer()
        {
            //arrange
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var question = Create.Entity.NumericIntegerQuestion(id: questionId, variable: "int", isPrefilled: true);
            var answer = NumericIntegerAnswer.FromInt(399);
            When_create_interview_with_identifier_data_Should_apply_answer(question, answer, "399");
        }

        [Test]
        public void When_create_interview_with_identifier_single_question_Should_apply_answer()
        {
            //arrange
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var question = Create.Entity.SingleOptionQuestion(questionId: questionId, variable: "single", answers: new List<Answer>()
            {
                Create.Entity.Answer("1", 1),
                Create.Entity.Answer("2", 2),
            }, isPrefilled: true);
            var answer = CategoricalFixedSingleOptionAnswer.FromInt(2);
            When_create_interview_with_identifier_data_Should_apply_answer(question, answer, "2");
        }

        [Test]
        public void When_create_interview_with_identifier_date_question_Should_apply_answer()
        {
            //arrange
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var question = Create.Entity.DateTimeQuestion(questionId: questionId, variable: "date", preFilled: true);
            var answer = DateTimeAnswer.FromDateTime(new DateTime(2017, 11, 15));
            When_create_interview_with_identifier_data_Should_apply_answer(question, answer, "2017-11-15");
        }

        [Test]
        public void When_create_interview_with_identifier_gps_question_Should_apply_answer()
        {
            //arrange
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var question = Create.Entity.GpsCoordinateQuestion(questionId: questionId, variable: "gps", isPrefilled: true);
            var answer = GpsAnswer.FromGeoPosition(new GeoPosition(2,2,2,2, DateTimeOffset.UtcNow));
            When_create_interview_with_identifier_data_Should_apply_answer(question, answer, "2,2[2]2");
        }

        private void When_create_interview_with_identifier_data_Should_apply_answer(IQuestion question, AbstractAnswer abstractAnswer, string stringAnswer)
        {
            //arrange
            var questionId = question.PublicKey;
            var questionIdentity = Create.Identity(questionId);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(question);
            var interview = Setup.StatefulInterview(questionnaire);

            var answersToIdentifyingQuestions = new List<InterviewAnswer>
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(questionId),
                    Answer = abstractAnswer
                }
            };

            var command = Create.Command.CreateInterview(interview.Id, answersToIdentifyingQuestions: answersToIdentifyingQuestions, assignmentId: 1);

            //act
            interview.CreateInterview(command);

            //assert
            Assert.That(interview.GetAnswerAsString(questionIdentity), Is.EqualTo(stringAnswer));
            Assert.That(interview.IsReadOnlyQuestion(questionIdentity), Is.True);
        }


        [Test]
        public void When_create_interview_from_assignment_Should_save_identify_answers_and_mark_its_as_readonly()
        {
            //arrange
            var questionTextId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionTextIdentity = Identity.Create(questionTextId, RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId: questionTextId, variable: "text", preFilled: true));
            var interview = Setup.StatefulInterview(questionnaire);

            var answersToIdentifyingQuestions = new List<InterviewAnswer>()
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(questionTextId),
                    Answer = TextAnswer.FromString("value")
                }
            };
            var command = Create.Command.CreateInterview(interview.Id, assignmentId: 1, answersToIdentifyingQuestions: answersToIdentifyingQuestions);

            //act
            interview.CreateInterview(command);

            //assert
            Assert.That(interview.GetAnswerAsString(questionTextIdentity), Is.EqualTo("value"));
            Assert.That(interview.IsReadOnlyQuestion(questionTextIdentity), Is.True);
        }

        [Test]
        public void When_Interview_in_status_SupervisorAssigned_And_interview_being_moved_to_other_team_As_result_supervisor_should_be_changed()
        {
            // arrange
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(Create.Entity.TextQuestion(questionId, preFilled: true));
            var interview = Setup.StatefulInterview(questionnaire);
            SetupEventContext();
            var answersToIdentifyingQuestions = new List<InterviewAnswer>()
            {
                new InterviewAnswer
                {
                    Identity = Create.Identity(questionId),
                    Answer = TextAnswer.FromString("value")
                }
            };
            var command = Create.Command.CreateInterview(interview.Id, assignmentId: 1, answersToIdentifyingQuestions: answersToIdentifyingQuestions);

            // act
            interview.CreateInterview(command);

            // assert
            eventContext.ShouldContainEvent<InterviewCreated>();
            eventContext.ShouldContainEvent<TextQuestionAnswered>(e => e.QuestionId == questionId);
            eventContext.ShouldContainEvent<QuestionsMarkedAsReadonly>(e => e.Questions.Single(i => i.Id == questionId) != null);
        }
    }
}
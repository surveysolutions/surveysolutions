using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefullInterviewTests
    {
        [Test]
        public void When_question_has_invalid_states_Should_be_exists_in_invalid_questions_collection()
        {
            var questionIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.TextQuestion(questionIdentity.Id,
                        validationConditions:
                        new List<Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()
                        {
                            new ValidationCondition("1=1", "invalid")
                        })
                }));
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(questionIdentity.Id, questionIdentity.RosterVector, "test"));

            // act
            statefulInterview.Apply(Create.Event.AnswersDeclaredInvalid(new[] { questionIdentity }));

            // assert
            Assert.That(statefulInterview.IsEntityValid(questionIdentity), Is.False);
            Assert.That(statefulInterview.CountInvalidEntitiesInInterview(), Is.EqualTo(1));
        }


        [Test]
        public void When_interview_has_invalid_prefield_question_invalid_questions_collection_should_have_it()
        {
            var questionIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.TextQuestion(questionIdentity.Id,
                        preFilled:true,
                        validationConditions:
                        new List<Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()
                        {
                            new ValidationCondition("1=1", "invalid")
                        })
                }));
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(questionIdentity.Id, questionIdentity.RosterVector, "test"));

            // act
            statefulInterview.Apply(Create.Event.AnswersDeclaredInvalid(new[] { questionIdentity }));

            // assert
            Assert.That(statefulInterview.IsEntityValid(questionIdentity), Is.False);
            Assert.That(statefulInterview.CountAllInvalidEntities(), Is.EqualTo(1));
        }

        [Test]
        public void When_question_has_readonly_and_invalid_states_Should_not_be_exists_in_invalid_questions_collection()
        {
            var questionIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.TextQuestion(questionIdentity.Id,
                        validationConditions:
                        new List<Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()
                        {
                            new ValidationCondition("1=1", "invalid")
                        })
                }));
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(questionIdentity.Id, questionIdentity.RosterVector, "test"));
            statefulInterview.Apply(Create.Event.QuestionsMarkedAsReadonly(DateTimeOffset.Now, questionIdentity));

            // act
            statefulInterview.Apply(Create.Event.AnswersDeclaredInvalid(new[] { questionIdentity }));

            // assert
            Assert.That(statefulInterview.IsEntityValid(questionIdentity), Is.False);
            Assert.That(statefulInterview.CountInvalidEntitiesInInterview(), Is.EqualTo(0));
        }
    }
}

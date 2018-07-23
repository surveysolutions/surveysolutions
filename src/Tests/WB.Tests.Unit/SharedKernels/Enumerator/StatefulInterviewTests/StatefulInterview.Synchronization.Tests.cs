using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefullInterviewTests
    {
        [Test]
        public void When_unanswered_question_has_comment_Should_add_to_comments_of_question()
        {
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneQuestion(questionId);

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var commentedAnswer = Create.Entity.AnsweredQuestionSynchronizationDto(questionId: questionId,
                comments: Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor));

            var syncDto = Create.Entity.InterviewSynchronizationDto(
                answers: new[] {commentedAnswer});

            interview.Synchronize(Create.Command.Synchronize(Guid.NewGuid(), syncDto));

            var questionComments = interview.GetQuestionComments(Create.Entity.Identity(questionId)).ToList();

            Assert.That(questionComments, Is.Not.Empty);
            Assert.That(questionComments, Has.Count.EqualTo(1));
        }

        [Test]
        public void When_rejected_interview_has_linked_to_list_and_text_list_question_is_answered_Should_linked_to_list_question_has_options()
        {
            //arrange
            var textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var multiLinkedToListId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.FixedRoster(fixedTitles: new[] {Create.Entity.FixedTitle(1)},
                    children: new[]
                    {
                        Create.Entity.MultyOptionsQuestion(multiLinkedToListId, linkedToQuestionId: textListQuestionId)
                    }));

            var interview = Setup.StatefulInterview(questionnaire);

            var answerOnTextList = Create.Entity.AnsweredQuestionSynchronizationDto(questionId: textListQuestionId,
                answer: new[] {new Tuple<decimal, string>(1, "option 1"), new Tuple<decimal, string>(5, "option 2")});

            var syncDto = Create.Entity.InterviewSynchronizationDto(answers: new[] { answerOnTextList });

            //act
            interview.Synchronize(Create.Command.Synchronize(Guid.NewGuid(), syncDto));

            //assert
            var multiLinkedToList = interview.GetMultiOptionLinkedToListQuestion(Create.Entity.Identity(multiLinkedToListId, Create.Entity.RosterVector(1)));

            Assert.That(multiLinkedToList.Options, Is.EquivalentTo(new[] {1, 5}));
        }

        [Test]
        public void When_rejected_interview_has_comment_for_removed_answer_Should_skip_that_comment_and_successfully_sync_without_exception()
        {
            //arrange
            var textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var textQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var groupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var staticTextId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.ListRoster(rosterSizeQuestionId: textListQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(textQuestionId),
                    Create.Entity.Group(groupId),
                    Create.Entity.StaticText(staticTextId)
                }));

            var interview = Setup.StatefulInterview(questionnaire);

            var answerOnTextList = Create.Entity.AnsweredQuestionSynchronizationDto(
                questionId: textListQuestionId,
                answer: new[] { new Tuple<decimal, string>(2, "option 2"), new Tuple<decimal, string>(5, "option 5") });

            var answerTextQuestion = Create.Entity.AnsweredQuestionSynchronizationDto(
                questionId: textQuestionId, 
                rosterVector:Create.Entity.RosterVector(1),
                answer: null, 
                comments: Create.Entity.CommentSynchronizationDto("comment"));

            var groupIdentity = Create.Entity.InterviewItemId( groupId, Create.Entity.RosterVector(1));
            var questionIdentity = Create.Entity.Identity( groupId, Create.Entity.RosterVector(1));
            var staticTextIdentity = Create.Entity.Identity( groupId, Create.Entity.RosterVector(1));

            var failedValidationConditions = new List<FailedValidationCondition> { Create.Entity.FailedValidationCondition(0) };

            var syncDto = Create.Entity.InterviewSynchronizationDto(
                answers: new[] { answerOnTextList, answerTextQuestion },
                disabledGroups: new HashSet<InterviewItemId> { groupIdentity },
                disabledQuestions: new HashSet<InterviewItemId> { groupIdentity },
                failedValidationConditions: new  Dictionary<Identity, IList<FailedValidationCondition>>
                {
                    { questionIdentity, failedValidationConditions } 
                },
                disabledStaticTexts: new List<Identity> { staticTextIdentity },
                invalidStaticTexts: new List<KeyValuePair<Identity, List<FailedValidationCondition>>>
                {
                    new KeyValuePair<Identity, List<FailedValidationCondition>>(staticTextIdentity, failedValidationConditions)
                }
               );

            //act
            TestDelegate sync = () => interview.ApplyEvent(new InterviewSynchronized(syncDto, DateTimeOffset.Now));

            //assert
            Assert.DoesNotThrow(sync);
        }

        [Test]
        public void When_restoring_interview_state_from_sync_package_having_legacy_values()
        {
            decimal[] rosterVector = new decimal[] { 1m, 0m };
            var fixedRosterIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"), Create.Entity.RosterVector(1));
            var fixedNestedRosterIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"), rosterVector);
            var fixedNestedNestedRosterIdentity = Identity.Create(Guid.Parse("33333333333333333333333333333333"),
                Create.Entity.RosterVector(1, 0, 3));

            var sourceOfLinkedQuestionId = Guid.Parse("00000000000000000000000000000042");
            var linkedSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000005");
            var multiOptionQuestionId = Guid.Parse("00000000000000000000000000000006");
            var linkedMultiOptionQuestionId = Guid.Parse("00000000000000000000000000000007");

            var linkedSingleOptionQuestionIntId = Guid.Parse("10000000000000000000000000000005");
            var multiOptionQuestionIntId = Guid.Parse("10000000000000000000000000000006");
            var linkedMultiOptionQuestionIntId = Guid.Parse("10000000000000000000000000000007");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.FixedRoster(fixedRosterIdentity.Id, variable: "r1", fixedTitles: new[] {new FixedRosterTitle(1, "fixed")},
                    children: new[]
                    {
                        Create.Entity.FixedRoster(fixedNestedRosterIdentity.Id, variable: "r2",
                            fixedTitles: new[] {new FixedRosterTitle(0, "fixed 2")},
                            children: new IComposite[]
                            {
                                Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, variable: "q1"),
                                Create.Entity.MultyOptionsQuestion(linkedMultiOptionQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId, variable: "q2"),
                                Create.Entity.SingleOptionQuestion(linkedSingleOptionQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId, variable: "q3"),
                                Create.Entity.MultyOptionsQuestion(multiOptionQuestionIntId, variable: "q4"),
                                Create.Entity.MultyOptionsQuestion(linkedMultiOptionQuestionIntId, linkedToQuestionId: sourceOfLinkedQuestionId, variable: "q5"),
                                Create.Entity.SingleOptionQuestion(linkedSingleOptionQuestionIntId, linkedToQuestionId: sourceOfLinkedQuestionId, variable: "q6"),
                                Create.Entity.FixedRoster(fixedNestedNestedRosterIdentity.Id, variable: "r3",
                                    fixedTitles: new[] {new FixedRosterTitle(3, "fixed 3")},
                                    children: new[]
                                    {
                                        Create.Entity.TextQuestion(sourceOfLinkedQuestionId, variable: "q7")
                                    })
                            })
                    })
            });


            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var multiOptionQuestionAnswer = new[] {1m};
            var linkedMultiOptionQuestionAnswer = new decimal[][] {new[] {1m}, new[] {2m}};
            var linkedSingleOptionQuestionAnswer = new decimal[] {1m};

            var multiOptionQuestionAnswerInt = new[] { 1 };
            var linkedMultiOptionQuestionAnswerInt = new int[][] { new[] { 1 }, new[] { 2 } };
            var linkedSingleOptionQuestionAnswerInt = new int[] { 1 };


            var answersDTOs = new[] {
                Create.Entity.AnsweredQuestionSynchronizationDto(multiOptionQuestionId, rosterVector, multiOptionQuestionAnswer),
                Create.Entity.AnsweredQuestionSynchronizationDto(linkedMultiOptionQuestionId, rosterVector, linkedMultiOptionQuestionAnswer),
                Create.Entity.AnsweredQuestionSynchronizationDto(linkedSingleOptionQuestionId, rosterVector, linkedSingleOptionQuestionAnswer),

                Create.Entity.AnsweredQuestionSynchronizationDto(multiOptionQuestionIntId, rosterVector, multiOptionQuestionAnswerInt),
                Create.Entity.AnsweredQuestionSynchronizationDto(linkedMultiOptionQuestionIntId, rosterVector, linkedMultiOptionQuestionAnswerInt),
                Create.Entity.AnsweredQuestionSynchronizationDto(linkedSingleOptionQuestionIntId, rosterVector, linkedSingleOptionQuestionAnswerInt),
            };

            var syncDto = Create.Entity.InterviewSynchronizationDto(answers: answersDTOs );

            interview.Synchronize(Create.Command.Synchronize(Guid.NewGuid(), syncDto));

            var multiOptionQuestion = interview.GetMultiOptionQuestion(new Identity(multiOptionQuestionId, rosterVector));
            var linkedMultiOptionQuestion = interview.GetLinkedMultiOptionQuestion(new Identity(linkedMultiOptionQuestionId, rosterVector));
            var linkedSingleOptionQuestion = interview.GetLinkedSingleOptionQuestion(new Identity(linkedSingleOptionQuestionId, rosterVector));

            var multiOptionQuestionInt = interview.GetMultiOptionQuestion(new Identity(multiOptionQuestionIntId, rosterVector));
            var linkedMultiOptionQuestionInt = interview.GetLinkedMultiOptionQuestion(new Identity(linkedMultiOptionQuestionIntId, rosterVector));
            var linkedSingleOptionQuestionInt = interview.GetLinkedSingleOptionQuestion(new Identity(linkedSingleOptionQuestionIntId, rosterVector));

            Assert.That(multiOptionQuestion.GetAnswer().ToDecimals(), Is.EqualTo(multiOptionQuestionAnswer));
            Assert.That(linkedMultiOptionQuestion.GetAnswer().CheckedValues, Is.EqualTo(linkedMultiOptionQuestionAnswer));
            Assert.That(linkedSingleOptionQuestion.GetAnswer().SelectedValue, Is.EqualTo(linkedSingleOptionQuestionAnswer));

            Assert.That(multiOptionQuestionInt.GetAnswer().ToDecimals(), Is.EqualTo(multiOptionQuestionAnswer));
            Assert.That(linkedMultiOptionQuestionInt.GetAnswer().CheckedValues, Is.EqualTo(linkedMultiOptionQuestionAnswer));
            Assert.That(linkedSingleOptionQuestionInt.GetAnswer().SelectedValue, Is.EqualTo(linkedSingleOptionQuestionAnswer));
        }


        [Test]
        public void When_rejected_interview_has_readonly_question_Should_restore_readonly_state_after_sync()
        {
            //arrange
            var textQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textQuestionIdentity = Create.Identity(textQuestionId);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(textQuestionId)
                );

            var interview = Setup.StatefulInterview(questionnaire);

            var answerTextQuestion = Create.Entity.AnsweredQuestionSynchronizationDto(
                questionId: textQuestionId,
                answer: "answer");

            var syncDto = Create.Entity.InterviewSynchronizationDto(
                answers: new[] { answerTextQuestion },
                readonlyQuestions: new HashSet<InterviewItemId> { Create.Entity.InterviewItemId(textQuestionId) }
            );

            //act
            interview.ApplyEvent(new InterviewSynchronized(syncDto, DateTimeOffset.Now));

            //assert
            Assert.That(interview.IsReadOnlyQuestion(textQuestionIdentity), Is.True);
        }
    }
}

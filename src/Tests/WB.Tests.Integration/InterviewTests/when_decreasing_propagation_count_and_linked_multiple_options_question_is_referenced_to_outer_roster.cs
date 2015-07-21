using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests
{
    internal class when_decreasing_propagation_count_and_linked_multiple_options_question_is_referenced_to_outer_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            answerTime = new DateTime(2013, 10, 02);

            numericQuestionId = Guid.Parse("11111111111111111111111111111111");
            var propagatableGroupId = Guid.Parse("22222222222222222222222222222222");
            var propagatableGroupId1 = Guid.Parse("22222222222222222222222222222221");

            referencedQuestionId = Guid.Parse("22220000000000000000000000000000");
            linkedQuestionId = Guid.Parse("33333333333333333333333333333333");

            var questionnaireDocument = new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                PublicKey = numericQuestionId,
                                IsInteger = true,
                                QuestionType = QuestionType.Numeric
                            },
                            new Group
                            {
                                IsRoster = true,
                                RosterSizeQuestionId = numericQuestionId, 
                                PublicKey = propagatableGroupId,
                                Propagated = Propagate.AutoPropagated,
                                Children = new List<IComposite>
                                {
                                    new TextQuestion
                                    {
                                        PublicKey = referencedQuestionId,
                                        QuestionType = QuestionType.Text,
                                    },
                                    new Group
                                    {
                                        IsRoster = true,
                                        RosterSizeQuestionId = numericQuestionId, 
                                        PublicKey = propagatableGroupId1,
                                        Propagated = Propagate.AutoPropagated,
                                        Children = new List<IComposite>
                                        {
                                            new MultyOptionsQuestion
                                            {
                                                PublicKey = linkedQuestionId,
                                                QuestionType = QuestionType.MultyOption,
                                                LinkedToQuestionId = referencedQuestionId,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            interview = CreateInterviewFromQuestionnaireDocumentRegisteringAllNeededDependencies(questionnaireDocument);
            interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, new decimal[] { }, answerTime, 3);
            interview.AnswerTextQuestion(userId, referencedQuestionId, new decimal[] { 0 }, answerTime, "A");
            interview.AnswerTextQuestion(userId, referencedQuestionId, new decimal[] { 2 }, answerTime, "C");
            interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedQuestionId, new decimal[] {0 , 0 }, answerTime,
                new decimal[][] { new decimal[] { 0 }, new decimal[] { 2 } });

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, new decimal[] { }, answerTime, 2);

        It should_raise_AnswersRemoved_event_with_source_for_linked_id_and_propagation_vector = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event => @event.Questions.Any(question
                => question.Id == referencedQuestionId
                    && question.RosterVector.SequenceEqual(new decimal[] { 2 })));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid numericQuestionId;
        private static DateTime answerTime;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;

    }
}
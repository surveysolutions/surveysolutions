using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests
{
    internal class when_decreasing_propagation_count_and_linked_multiple_options_question_is_referenced_to_outer_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            answerTime = new DateTime(2013, 10, 02);

            numericQuestionId = Guid.Parse("11111111111111111111111111111111");
            var propagatableGroupId = Guid.Parse("22222222222222222222222222222222");
            var propagatableGroupId1 = Guid.Parse("22222222222222222222222222222221");

            referencedQuestionId = Guid.Parse("22220000000000000000000000000000");
            linkedQuestionId = Guid.Parse("33333333333333333333333333333333");

            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                            new NumericQuestion
                            {
                                PublicKey = numericQuestionId,
                                IsInteger = true,
                                StataExportCaption = "num"
                            },
                            new Group
                            {
                                IsRoster = true,
                                RosterSizeQuestionId = numericQuestionId, 
                                PublicKey = propagatableGroupId,
                                VariableName = "ros",
                                Children = new List<IComposite>
                                {
                                    new TextQuestion
                                    {
                                        PublicKey = referencedQuestionId,
                                        StataExportCaption = "txt"
                                    },
                                    new Group
                                    {
                                        IsRoster = true,
                                        RosterSizeQuestionId = numericQuestionId, 
                                        PublicKey = propagatableGroupId1,
                                        VariableName = "ros2",
                                        Children = new List<IComposite>
                                        {
                                            new MultyOptionsQuestion
                                            {
                                                PublicKey = linkedQuestionId,
                                                LinkedToQuestionId = referencedQuestionId,
                                                StataExportCaption = "link_mul"
                                            }
                                        }.ToReadOnlyCollection()
                                    }
                                }.ToReadOnlyCollection()
                            });
            appDomainContext = AppDomainContext.Create();

            interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument);
            interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, new decimal[] { }, answerTime, 3);
            interview.AnswerTextQuestion(userId, referencedQuestionId, new decimal[] { 0 }, answerTime, "A");
            interview.AnswerTextQuestion(userId, referencedQuestionId, new decimal[] { 2 }, answerTime, "C");
            interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedQuestionId, new decimal[] {0 , 0 }, answerTime,
                new RosterVector[] { new decimal[] { 0 }, new decimal[] { 2 } });

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, new decimal[] { }, answerTime, 2);

        [NUnit.Framework.Test] public void should_raise_AnswersRemoved_event_with_source_for_linked_id_and_propagation_vector () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event => @event.Questions.Any(question
                => question.Id == referencedQuestionId && question.RosterVector.Identical(Abc.Create.RosterVector(2))));

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
            appDomainContext.Dispose();
        }

        private static AppDomainContext appDomainContext;
        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid numericQuestionId;
        private static DateTime answerTime;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
    }
}

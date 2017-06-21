﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Ignore("C#, KP-4390 Interview creation")]
    internal class when_creating_interview_and_questionnaire_has_question_with_custom_enablement_condition_and_question_has_no_propagatable_ancestor_groups : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var answersToFeaturedQuestions = new List<InterviewAnswer>();
            answersTime = new DateTime(2013, 09, 01);

            questionId = Guid.Parse("22220000111111111111111111111111");

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetRostersFromTopToSpecifiedQuestion(questionId) == new Guid[] {});

            var questionnaireRepository = Stub<IQuestionnaireStorage>.Returning(questionaire);

            eventContext = new EventContext();

            command = Create.Command.CreateInterviewCommand(questionnaireId, 1, supervisorId,
                answersToFeaturedQuestions, answersTime: answersTime, userId: userId);
            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            interview.CreateInterviewWithPreloadedData(command);

        It should_raise_QuestionsDisabled_event = () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>();

        It should_provide_id_of_group_with_custom_enablement_condition_in_QuestionsDisabled_event = () =>
            eventContext.GetEvent<QuestionsDisabled>().Questions.Single()
                .Id.ShouldEqual(questionId);

        It should_provide_zero_dimensional_propagation_vector_in_QuestionsDisabled_event = () =>
            eventContext.GetEvent<QuestionsDisabled>().Questions.Single()
                .RosterVector.Length.ShouldEqual(0);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid questionId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Interview interview;
        private static CreateInterviewWithPreloadedData command;
    }
}
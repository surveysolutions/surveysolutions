using System;
using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_question_with_custom_enablement_condition_and_question_has_propagatable_ancestor_group : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            answersToFeaturedQuestions = new Dictionary<Guid, AbstractAnswer>();
            answersTime = new DateTime(2013, 09, 01);

            Guid questionId = Guid.Parse("22220000111111111111111111111111");
            Guid parentPropagatableGroupId = Guid.Parse("22220000AAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), _
                => /*_.GetAllQuestionsWithNotEmptyCustomEnablementConditions() == new [] { questionId }
                && */_.GetRostersFromTopToSpecifiedQuestion(questionId) == new [] { parentPropagatableGroupId });

            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            interview.CreateInterview(questionnaireId, 1, supervisorId, answersToFeaturedQuestions, answersTime, userId);

        It should_not_raise_QuestionDisabled_event = () =>
            eventContext.ShouldNotContainEvent<QuestionsDisabled>();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Interview interview;
    }
}
using System;
using System.Collections.Generic;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_question_with_custom_enablement_condition_and_question_has_no_propagatable_ancestor_groups : InterviewTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("11111111111111111111111111111111");
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();
            answersTime = new DateTime(2013, 09, 01);

            questionId = Guid.Parse("22220000111111111111111111111111");

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetAllQuestionsWithNotEmptyCustomEnablementConditions() == new [] { questionId }
                && _.GetParentPropagatableGroupsForQuestionStartingFromTop(questionId) == new Guid[] {});

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            new Interview(interviewId, userId, questionnaireId, answersToFeaturedQuestions, answersTime, supervisorId);

        It should_raise_QuestionDisabled_event = () =>
            eventContext.ShouldContainEvent<QuestionDisabled>();

        It should_provide_id_of_group_with_custom_enablement_condition_in_QuestionDisabled_event = () =>
            GetEvent<QuestionDisabled>(eventContext)
                .QuestionId.ShouldEqual(questionId);

        It should_provide_zero_dimensional_propagation_vector_in_QuestionDisabled_event = () =>
            GetEvent<QuestionDisabled>(eventContext)
                .PropagationVector.Length.ShouldEqual(0);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid questionId;
        private static Guid interviewId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
        private static DateTime answersTime;
        private static Guid supervisorId;
    }
}
﻿using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_InterviewOnClientCreated_event_recived : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("12222222222222222222222222222222");

            questionnaireId = Guid.Parse("93333333333333333333333333333333");
            questionnaireVersion = 1;

            var userDocument = Create.Entity.UserView(userId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(user: userDocument);
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(null,
                CreatePublishableEvent(new InterviewOnClientCreated(userId, questionnaireId, questionnaireVersion)));

        private It should_interview_responsible_be_equal_to_creator_id = () =>
            viewState.ResponsibleId.ShouldEqual(userId);

        private It should_questionnareid_be_equal_to_provided_questionnaire_id = () =>
            viewState.QuestionnaireId.ShouldEqual(questionnaireId);

        private It should_questionnaire_version_equal_to_provided_questionnaire_version = () =>
            viewState.QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        It should_interview_levels_count_be_equal_to_1 = () =>
            viewState.Levels.Keys.Count.ShouldEqual(1);

        It should_interview_has_created_onclient_true = () =>
            viewState.CreatedOnClient.ShouldEqual(true);


        private static Guid questionnaireId;
        private static long questionnaireVersion;

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid userId;
    }
}

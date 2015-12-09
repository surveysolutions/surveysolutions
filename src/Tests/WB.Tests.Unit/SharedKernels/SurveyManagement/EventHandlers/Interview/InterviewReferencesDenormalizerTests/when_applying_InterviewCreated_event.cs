using System;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewReferencesDenormalizerTests
{
    internal class when_applying_InterviewCreated_event
    {
        Establish context = () =>
        {
            @event = Create.Event
                .InterviewCreated(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion)
                .ToPublishedEvent(eventSourceId: eventSourceId);

            denormalizer = Create.InterviewReferencesDenormalizer();
        };

        Because of = () =>
            resultView = denormalizer.Update(null, @event);

        It should_return_view_with_interview_id_equal_to_event_source_id = () =>
            resultView.InterviewId.ShouldEqual(eventSourceId);

        It should_return_view_with_questionnaire_id_equal_to_questionnaire_id_from_event = () =>
            resultView.QuestionnaireId.ShouldEqual(questionnaireId);

        It should_return_view_with_questionnaire_version_equal_to_questionnaire_version_from_event = () =>
            resultView.QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        private static InterviewReferences resultView;
        private static InterviewReferencesDenormalizer denormalizer;
        private static IPublishedEvent<InterviewCreated> @event;
        private static Guid eventSourceId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 404;
    }
}
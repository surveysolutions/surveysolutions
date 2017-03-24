using System;
using Machine.Specifications;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewReferencesDenormalizerTests
{
    internal class when_applying_InterviewFromPreloadedDataCreated_event
    {
        Establish context = () =>
        {
            @event = Create.Event
                .InterviewFromPreloadedDataCreated(questionnaireId: questionnaireId, questionnaireVersion: questionnaireVersion)
                .ToPublishedEvent(eventSourceId: eventSourceId);

            denormalizer = Create.Service.InterviewReferencesDenormalizer();
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
        private static IPublishedEvent<InterviewFromPreloadedDataCreated> @event;
        private static Guid eventSourceId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion = 404;
    }
}
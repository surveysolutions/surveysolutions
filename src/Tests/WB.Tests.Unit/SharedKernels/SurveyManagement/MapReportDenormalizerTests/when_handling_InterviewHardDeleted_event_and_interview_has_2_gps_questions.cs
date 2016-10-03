using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.MapReportDenormalizerTests
{
    internal class when_handling_InterviewHardDeleted_event_and_interview_has_2_gps_questions
    {
        Establish context = () =>
        {
            @event = Create.PublishedEvent.InterviewHardDeleted(interviewId: interviewId);

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                Create.Entity.GpsCoordinateQuestion(variable: gpsVariable1),
                Create.Entity.GpsCoordinateQuestion(variable: gpsVariable2),
            });

            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage = Setup.ReadSideKeyValueStorageWithSameEntityForAnyGet(Create.Entity.InterviewReferences());

            mapReportPointStorage.Store(Create.Entity.MapReportPoint(markerId1, 1, 3), markerId1);
            mapReportPointStorage.Store(Create.Entity.MapReportPoint(markerId2, 2, 4), markerId2);

            denormalizer = Create.Service.MapReportDenormalizer(
                mapReportPointStorage: mapReportPointStorage,
                interviewReferencesStorage: interviewReferencesStorage,
                questionnaireDocument: questionnaireDocument);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_delete_all_gps_question_map_report_point_for_deleted_interview = () =>
            mapReportPointStorage.Count().ShouldEqual(0);

        private static MapReportDenormalizer denormalizer;
        private static IPublishedEvent<InterviewHardDeleted> @event;
        private static readonly TestInMemoryWriter<MapReportPoint> mapReportPointStorage = Create.Storage.InMemoryReadeSideStorage<MapReportPoint>();
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string gpsVariable1 = "gps1";
        private static readonly string gpsVariable2 = "gps2";
        private static readonly string markerId1 = $"{interviewId}-{gpsVariable1}";
        private static readonly string markerId2 = $"{interviewId}-{gpsVariable2}";
    }
}
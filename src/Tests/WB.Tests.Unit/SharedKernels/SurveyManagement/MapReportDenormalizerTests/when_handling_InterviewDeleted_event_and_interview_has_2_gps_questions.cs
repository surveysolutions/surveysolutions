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
    internal class when_handling_InterviewDeleted_event_and_interview_has_2_gps_questions
    {
        Establish context = () =>
        {
            @event = Create.PublishedEvent.InterviewDeleted(interviewId: interviewId);

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

        It should_remove_all_points_from_storage = () =>
            mapReportPointStorage.Count().ShouldEqual(0);

        private static MapReportDenormalizer denormalizer;
        private static IPublishedEvent<InterviewDeleted> @event;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string gpsVariable1 = "gps1";
        private static string gpsVariable2 = "gps2";
        private static readonly TestInMemoryWriter<MapReportPoint> mapReportPointStorage = Create.Storage.InMemoryReadeSideStorage<MapReportPoint>();
        private static readonly string markerId1 = $"{interviewId}-{gpsVariable1}";
        private static readonly string markerId2 = $"{interviewId}-{gpsVariable2}";
    }
}
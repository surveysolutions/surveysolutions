using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.UserViewFactoryTests
{
    internal class when_hq_sorting_by_deviceId : UserViewFactoryTestContext
    {
        [Test]
        public void should_sort_by_device_id_and_filter_by_outdated_app()
        {
            var headquarter1 = Create.Entity.HqUser(Id.g7, userName: "headquarter1", role: UserRoles.Headquarter);

            Guid supervisor1Id = Id.g1;
            var supervisor1 = Create.Entity.HqUser(supervisor1Id, userName: "supervisor1", role: UserRoles.Supervisor);
            var interviewer11 = Create.Entity.HqUser(Id.gA, supervisor1Id, userName: "interviewer11", deviceId: "device11", interviewerBuild: 99);
            var interviewer12 = Create.Entity.HqUser(Id.gB, supervisor1Id, userName: "interviewer12", interviewerBuild: 100);

            Guid supervisor2Id = Id.g2;
            var supervisor2 = Create.Entity.HqUser(supervisor2Id, null, userName: "supervisor2", role: UserRoles.Supervisor);
            var interviewer21 = Create.Entity.HqUser(Id.gC, supervisor2Id, userName: "interviewer21", deviceId: "device21", interviewerBuild: 99);

            Guid supervisor3Id = Id.g3;
            var supervisor3 = Create.Entity.HqUser(supervisor3Id, null, userName: "supervisor3", role: UserRoles.Supervisor);
            var interviewer31 = Create.Entity.HqUser(Id.gE, supervisor3Id, userName: "interviewer31", isArchived: true, deviceId: "device31", interviewerBuild: 100);
            var interviewer32 = Create.Entity.HqUser(Id.g4, supervisor3Id, userName: "interviewer32", interviewerBuild: 100);
            var interviewer33 = Create.Entity.HqUser(Id.g5, supervisor3Id, userName: "interviewer33", deviceId: "device33", interviewerBuild: 99);
            var interviewer34 = Create.Entity.HqUser(Id.g6, supervisor3Id, userName: "interviewer34", isArchived: true, interviewerBuild: 100);

            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(
                headquarter1,
                supervisor1, supervisor2, supervisor3,
                interviewer11, interviewer12,
                interviewer21,
                interviewer31, interviewer32, interviewer33, interviewer34)
                .WithDeviceInfo();

            var interviewersViewFactory = CreateInterviewersViewFactory(readerWithUsers);

           // Act
            var result = interviewersViewFactory.GetInterviewers(0, 20, "DeviceId", null, false, 100, null, InterviewerFacet.OutdatedApp);

            result.Items.Should().HaveCount(3);

            result.Items.Skip(0).First().DeviceId.Should().Be("device11");
            result.Items.Skip(1).First().DeviceId.Should().Be("device21");
            result.Items.Skip(2).First().DeviceId.Should().Be("device33");
        }
    }
}

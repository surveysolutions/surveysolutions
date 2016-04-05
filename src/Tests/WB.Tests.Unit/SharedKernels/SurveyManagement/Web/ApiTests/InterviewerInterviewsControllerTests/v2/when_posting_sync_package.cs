﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class when_posting_sync_package : InterviewsApiV2ControllerTestsContext
    {
        private Establish context = () =>
        {
            controller = CreateInterviewerInterviewsController(
                incomingSyncPackagesQueue: mockOfInterviewPackagesService.Object);
        };

        Because of = () => controller.Post(new InterviewPackageApiView { InterviewId = interviewId, Events = eventsInJsonString, MetaInfo = interviewMetaInfo});

        It should_store_package_to_storage = () =>
            mockOfInterviewPackagesService.Verify(x =>
                x.StorePackage(interviewId, interviewMetaInfo.TemplateId, interviewMetaInfo.TemplateVersion,
                    interviewMetaInfo.ResponsibleId, (InterviewStatus) interviewMetaInfo.Status,
                    interviewMetaInfo.CreatedOnClient.Value, eventsInJsonString), Times.Once);

        private static InterviewsApiV2Controller controller;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string eventsInJsonString = "serialized events";
        private static readonly InterviewMetaInfo interviewMetaInfo = new InterviewMetaInfo
        {
            TemplateId = Guid.Parse("22222222222222222222222222222222"),
            TemplateVersion = 111,
            Status = (int)InterviewStatus.Restarted,
            ResponsibleId = Guid.Parse("33333333333333333333333333333333"),
            CreatedOnClient = true
        };
        private static readonly Mock<IInterviewPackagesService> mockOfInterviewPackagesService = new Mock<IInterviewPackagesService>();
    }
}

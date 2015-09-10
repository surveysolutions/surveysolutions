﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_delete_interview : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            syncItem = new InterviewSyncPackageDto
                       {
                           Content = interviewId.ToString(), MetaInfo = "some metadata"
                       };

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            cleanUpExecutorMock = new Mock<ICapiCleanUpService>();


            var viewFactory = new Mock<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>();
            var existingInterview = new InterviewMetaInfo
            {
                ResponsibleId = responsibleId
            };
            viewFactory.SetReturnsDefault(existingInterview);

            var userIdentity = Mock.Of<IUserIdentity>(x => x.UserId == responsibleId);
            var principal = Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == userIdentity);

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(commandService: commandService.Object, 
                plainQuestionnaireRepository: plainQuestionnaireRepositoryMock.Object, 
                capiCleanUpService: cleanUpExecutorMock.Object,
                interviewMetaInfoFactory: viewFactory.Object, principal: principal);
        };

        Because of = () => capiDataSynchronizationService.ProcessDownloadedInterviewPackages(syncItem, SyncItemType.DeleteInterview);

        It should_never_call_any_command =
            () => commandService.Verify(x => x.Execute(Moq.It.IsAny<ICommand>(), null), Times.Never);

        It should_cleanup_data_for_interview =
            () => cleanUpExecutorMock.Verify(x => x.DeleteInterview(interviewId), Times.Once);

        static CapiDataSynchronizationService capiDataSynchronizationService;
        static InterviewSyncPackageDto syncItem;
        static Mock<ICommandService> commandService;
        static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        static Mock<ICapiCleanUpService> cleanUpExecutorMock;
        static Guid interviewId;
        static Guid responsibleId;
    }
}

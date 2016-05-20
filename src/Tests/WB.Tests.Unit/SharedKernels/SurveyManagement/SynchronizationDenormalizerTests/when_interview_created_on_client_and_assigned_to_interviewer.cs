﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization.SyncStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    internal class when_interview_created_on_client_and_assigned_to_interviewer : InterviewSynchronizationDenormalizerTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            denormalizer = CreateDenormalizer(interviewPackageStorageWriter: interviewPackageStorageWriterMock.Object);
        };

        Because of = () => denormalizer.Handle(Create.Other.InterviewStatusChangedEvent(InterviewStatus.Completed));

        It should_not_store_any_sync_package = () =>
            interviewPackageStorageWriterMock.Verify(x => x.Store(Moq.It.IsAny<InterviewSyncPackageMeta>(), Moq.It.IsAny<string>()), Times.Never);

        static InterviewSynchronizationDenormalizer denormalizer;
        private static Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>> interviewPackageStorageWriterMock = new Mock<IReadSideRepositoryWriter<InterviewSyncPackageMeta>>();
        static Guid interviewId;
    }
}


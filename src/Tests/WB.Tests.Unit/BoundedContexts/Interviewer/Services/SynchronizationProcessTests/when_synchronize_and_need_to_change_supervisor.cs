﻿using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestFixture]
    public class when_synchronize_and_need_to_change_supervisor
    {
        [OneTimeSetUp]
        public void Context()
        {
            var interviewerIdentity = new InterviewerIdentity {Name = "name", PasswordHash = "hash", Token = "Outdated token", SupervisorId = Id.g1};

            PrincipalMock = Mock.Get(SetUp.InterviewerPrincipal(interviewerIdentity));
            PrincipalMock.Setup(x => x.GetInterviewerByName(It.IsAny<string>())).Returns(interviewerIdentity);
            
            SynchronizationServiceMock
                .Setup(x => x.GetCurrentSupervisor(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Id.gA);

            
            viewModel = Create.Service.SynchronizationProcess(principal: PrincipalMock.Object,
                synchronizationService: SynchronizationServiceMock.Object);

            viewModel
                .SynchronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None)
                .WaitAndUnwrapException();
        }

        [Test]
        public void should_store_updated_supervisor_id_in_plain_storage()
        {
            PrincipalMock.Verify(
                x => x.SaveInterviewer(It.Is<InterviewerIdentity>(i => i.SupervisorId == Id.gA)), Times.Once);

            PrincipalMock.Verify(x => x.SignInWithHash("name", "hash", true), Times.Once);
        }

        static InterviewerOnlineSynchronizationProcess viewModel;
        
        static Mock<IInterviewerPrincipal> PrincipalMock = new Mock<IInterviewerPrincipal>();
        static readonly Mock<IOnlineSynchronizationService>  SynchronizationServiceMock = new Mock<IOnlineSynchronizationService>();
    }
}

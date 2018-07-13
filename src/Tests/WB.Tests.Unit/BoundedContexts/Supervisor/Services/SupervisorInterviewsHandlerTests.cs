using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Events;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorInterviewsHandler))]
    public class SupervisorInterviewsHandlerTests
    {
        [Test]
        public async Task CanSynchronize_should_check_assemblyFileVersion_for_compatibility()
        {
            var handler = Create.Service.SupervisorInterviewsHandler();

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.Handle(new CanSynchronizeRequest(expectedVersion.Revision));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).True);
        }

        [Test]
        public async Task CanSynchronize_should_check_assemblyFileVersion_for_incompatibility()
        {
            var handler = Create.Service.SupervisorInterviewsHandler();

            var response = await handler.Handle(new CanSynchronizeRequest(1));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
        }

        [Test]
        public async Task GetInterviewsRequest_Should_filter_interviews_by_status_and_responsible()
        {
            var interviewerId = Id.gA;
            var questionnaireId = Create.Entity.QuestionnaireIdentity().ToString();
            var interviews = new SqliteInmemoryStorage<InterviewView>();
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1,responsibleId: interviewerId, status: InterviewStatus.Completed, questionnaireId: questionnaireId));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g2,responsibleId: interviewerId, status: InterviewStatus.RejectedBySupervisor, questionnaireId: questionnaireId));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g3,responsibleId: Id.gB, status: InterviewStatus.RejectedBySupervisor, questionnaireId: questionnaireId));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g4,responsibleId: interviewerId, status: InterviewStatus.RejectedByHeadquarters, questionnaireId: questionnaireId));
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g5,responsibleId: interviewerId, status: InterviewStatus.InterviewerAssigned, questionnaireId: questionnaireId));

            var handler = Create.Service.SupervisorInterviewsHandler(interviews: interviews);

            // Act
            var response = await handler.Handle(new GetInterviewsRequest(interviewerId));

            response.Interviews.Select(x => x.Id).Should().BeEquivalentTo(Id.g2, Id.g5);
        }

        [Test]
        public async Task LogInterviewAsSuccessfullyHandledRequest_Should_hide_interview_from_dashboard()
        {
            var interviews = new SqliteInmemoryStorage<InterviewView>();
            interviews.Store(Create.Entity.InterviewView(status: InterviewStatus.RejectedBySupervisor, interviewId: Id.g1));

            var handler = Create.Service.SupervisorInterviewsHandler(interviews: interviews);

            // Act
            var response = await handler.Handle(new LogInterviewAsSuccessfullyHandledRequest(Id.g1));

            // Assert
            interviews.Where(x => x.InterviewId == Id.g1).Count.Should().Be(0);
            Assert.That(response, Is.Not.Null);
        }

        [Test]
        public async Task UploadInterview_should_apply_sync_packge_when_its_fine()
        {
            var commandSerivce = new Mock<ICommandService>();
            var serializer = new Mock<IJsonAllTypesSerializer>();

            var packageEvents = new[]
            {
                Create.Event.AggregateRootEvent(Create.Event.InterviewCreated())
            };
            var interviewId = packageEvents[0].EventSourceId;

            var packageSerializedEvents = "test events";
            serializer.Setup(x => x.Deserialize<AggregateRootEvent[]>(packageSerializedEvents))
                .Returns(packageEvents);

            var handler = Create.Service.SupervisorInterviewsHandler(commandService: commandSerivce.Object,
                serializer: serializer.Object);

            // Act
            var response = await handler.UploadInterview(new UploadInterviewRequest
            {
                Interview = new InterviewPackageApiView
                {
                    Events = packageSerializedEvents,
                    InterviewId = interviewId,
                    MetaInfo = new InterviewMetaInfo()
                },
                InterviewKey = "124"
            });

            // assert
            Assert.That(response, Is.Not.Null);

            commandSerivce.Verify(x => x.Execute(It.Is<SynchronizeInterviewEventsCommand>(c =>
                c.InterviewId == interviewId &&
                       c.InterviewKey.Equals(new InterviewKey(124)) &&
                       c.SynchronizedEvents[0].GetType() == typeof(InterviewCreated)
            ), null));
        }

        [Test]
        public async Task UploadInterview_should_throw_interview_exception_when_same_sync_package_is_sent_twice()
        {
            var commandSerivce = new Mock<ICommandService>();
            var serializer = new Mock<IJsonAllTypesSerializer>();

            var packageEvents = new[]
            {
                Create.Event.AggregateRootEvent(Create.Event.InterviewCreated()),
                Create.Event.AggregateRootEvent(Create.Event.InteviewCompleted())
            };
            var interviewId = packageEvents[0].EventSourceId;

            var packageSerializedEvents = "test events";
            serializer.Setup(x => x.Deserialize<AggregateRootEvent[]>(packageSerializedEvents))
                .Returns(packageEvents);

            var brokenStorageMock = new Mock<IPlainStorage<BrokenInterviewPackageView, int?>>();

            var handler = Create.Service.SupervisorInterviewsHandler(
                commandService: commandSerivce.Object,
                serializer: serializer.Object,
                brokenInterviewStorage: brokenStorageMock.Object);

            // Act
            var request = new UploadInterviewRequest
            {
                Interview = new InterviewPackageApiView
                {
                    Events = packageSerializedEvents,
                    InterviewId = interviewId,
                    MetaInfo = new InterviewMetaInfo()
                },
                InterviewKey = "124"
            };
            await handler.UploadInterview(request);
            await handler.UploadInterview(request);

            // assert
            brokenStorageMock.Verify(bs => bs.Store(It.Is<BrokenInterviewPackageView>(bpv =>
                bpv.ExceptionType == InterviewDomainExceptionType.DuplicateSyncPackage.ToString()                
            )), Times.Once);
        }

        [Test]
        public async Task UploadInterviewRequest_should_store_as_broken_interviewe_if_was_ArgumentException()
        {
            var serializer = new Mock<IJsonAllTypesSerializer>();
            serializer.Setup(x => x.Deserialize<AggregateRootEvent[]>(It.IsAny<string>()))
                .Throws(new ArgumentException("test"));

            var brokenStorageMock = new Mock<IPlainStorage<BrokenInterviewPackageView, int?>>();

            var handler = Create.Service.SupervisorInterviewsHandler(
                serializer: serializer.Object,
                brokenInterviewStorage: brokenStorageMock.Object);

            // Act
            var request = new UploadInterviewRequest
            {
                Interview = new InterviewPackageApiView()
                {
                    Events = "events",
                    MetaInfo = new InterviewMetaInfo()
                }
            };
            await handler.UploadInterview(request);

            // assert
            brokenStorageMock.Verify(bs => bs.Store(It.Is<BrokenInterviewPackageView>(bpv =>
                bpv.ExceptionType == SupervisorInterviewsHandler.UnknownExceptionType                
            )), Times.Once);
        }

        [Test]
        public async Task UploadInterviewRequest_should_store_as_broken_interviewe_if_was_InterviewException_with_specified_type()
        {
            var interviewDomainExceptionType = InterviewDomainExceptionType.ExpessionCalculationError;
            var serializer = new Mock<IJsonAllTypesSerializer>();
            serializer.Setup(x => x.Deserialize<AggregateRootEvent[]>(It.IsAny<string>()))
                .Throws(new InterviewException("test", interviewDomainExceptionType));

            var brokenStorageMock = new Mock<IPlainStorage<BrokenInterviewPackageView, int?>>();

            var handler = Create.Service.SupervisorInterviewsHandler(
                serializer: serializer.Object,
                brokenInterviewStorage: brokenStorageMock.Object);

            // Act
            var request = new UploadInterviewRequest
            {
                Interview = new InterviewPackageApiView()
                {
                    Events = "events",
                    MetaInfo = new InterviewMetaInfo()
                }
            };
            await handler.UploadInterview(request);

            // assert
            brokenStorageMock.Verify(bs => bs.Store(It.Is<BrokenInterviewPackageView>(bpv =>
                bpv.ExceptionType == interviewDomainExceptionType.ToString()
            )), Times.Once);
        }

        [Test]
        public void UnknownExceptionType_are_equal_in_SupervisorInterviewsHandler_and_InterviewPackagesService()
        {
            Assert.AreEqual(SupervisorInterviewsHandler.UnknownExceptionType, InterviewPackagesService.UnknownExceptionType);
        }
    }
}

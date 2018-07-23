using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Supervisor;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
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
            var userId = Guid.NewGuid();
            var userToken = "test token";
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument(){Token = userToken});

            var handler = Create.Service.SupervisorInterviewsHandler(interviewerViewRepository:users.Object);

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.Handle(new CanSynchronizeRequest(expectedVersion.Revision, userId, userToken));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).True);
        }

        [Test]
        public async Task CanSynchronize_should_check_user_Token()
        {
            var userId = Guid.NewGuid();
            var userToken = "test token";
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument() { Token = userToken });

            var handler = Create.Service.SupervisorInterviewsHandler(interviewerViewRepository: users.Object);

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.Handle(new CanSynchronizeRequest(expectedVersion.Revision, userId, "new token"));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
            Assert.AreEqual(response.Reason, SyncDeclineReason.InvalidPassword);
        }

        [Test]
        public async Task CanSynchronize_should_check_UserId_for_Team_belonging()
        {
            var userId = Guid.NewGuid();
            var users = new Mock<IPlainStorage<InterviewerDocument>>();
            users.Setup(x => x.GetById(userId.FormatGuid())).Returns(new InterviewerDocument());

            var handler = Create.Service.SupervisorInterviewsHandler(interviewerViewRepository: users.Object);

            var expectedVersion = ReflectionUtils.GetAssemblyVersion(typeof(SupervisorBoundedContextAssemblyIndicator));
            var response = await handler.Handle(new CanSynchronizeRequest(expectedVersion.Revision, Guid.NewGuid(), String.Empty));

            Assert.That(response, Has.Property(nameof(response.CanSyncronize)).False);
            Assert.AreEqual(response.Reason, SyncDeclineReason.NotATeamMember);
        }

        [Test]
        public async Task CanSynchronize_should_check_assemblyFileVersion_for_incompatibility()
        {
            var handler = Create.Service.SupervisorInterviewsHandler();

            var response = await handler.Handle(new CanSynchronizeRequest(1, Guid.NewGuid(), String.Empty));

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
            response.Interviews.Select(x => x.ResponsibleId).Should().BeEquivalentTo(interviewerId, interviewerId);
        }

        [Test]
        public async Task GetInterviewsRequest_Should_return_responsible_in_response()
        {
            var responsibleId = Id.g1;
            var questionnaireId = Create.Entity.QuestionnaireIdentity().ToString();

            var interviews = new SqliteInmemoryStorage<InterviewView>();
            interviews.Store(Create.Entity.InterviewView(interviewId: Id.g1, responsibleId: responsibleId, status: InterviewStatus.RejectedBySupervisor, questionnaireId: questionnaireId));

            var handler = Create.Service.SupervisorInterviewsHandler(interviews: interviews);

            // Act
            var response = await handler.Handle(new GetInterviewsRequest(responsibleId));

            response.Interviews.Select(x => x.ResponsibleId).Should().BeEquivalentTo(responsibleId);
        }

        [Test]
        public async Task LogInterviewAsSuccessfullyHandledRequest_Should_not_delete_interview()
        {
            var interviews = new SqliteInmemoryStorage<InterviewView>();
            interviews.Store(Create.Entity.InterviewView(status: InterviewStatus.RejectedBySupervisor, interviewId: Id.g1));

            var handler = Create.Service.SupervisorInterviewsHandler(interviews: interviews);

            // Act
            var response = await handler.Handle(new LogInterviewAsSuccessfullyHandledRequest(Id.g1));

            // Assert
            var interviewView = interviews.GetById(Id.g1.FormatGuid());
            interviewView.Should().Should().NotBeNull();
            interviewView.ReceivedByInterviewerAtUtc.Should().BeWithin(TimeSpan.FromSeconds(3)).Before(DateTime.UtcNow);
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
        public async Task UploadInterview_should_apply_sync_packge_and_change_supervisor_if_team_changed()
        {
            var commandSerivce = new Mock<ICommandService>();
            var serializer = new Mock<IJsonAllTypesSerializer>();

            var packageEvents = new[]
            {
                Create.Event.AggregateRootEvent(Create.Event.InterviewCreated()),
                Create.Event.AggregateRootEvent(Create.Event.SupervisorAssigned(Id.g1, Id.g2))
            };
            var interviewId = packageEvents[0].EventSourceId;

            var packageSerializedEvents = "test events"; 
            serializer.Setup(x => x.Deserialize<AggregateRootEvent[]>(packageSerializedEvents))
                .Returns(packageEvents);

            var principal = new Mock<IPrincipal>();
            principal.Setup(x => x.CurrentUserIdentity).Returns(new SupervisorIdentity() {UserId = Id.g7});

            var handler = Create.Service.SupervisorInterviewsHandler(commandService: commandSerivce.Object,
                serializer: serializer.Object, principal:principal.Object);

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
                c.SynchronizedEvents[0].GetType() == typeof(InterviewCreated) &&
                c.NewSupervisorId == Id.g7
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
        public async Task UploadInterview_should_update_quantity_on_assignment_when_interview_received()
        {
            var assignmentId = 1;
            var existingInterviews = new InMemoryPlainStorage<InterviewView>();
            existingInterviews.Store(Create.Entity.InterviewView(assignmentId: assignmentId, interviewId: Id.g1));
            existingInterviews.Store(Create.Entity.InterviewView(assignmentId: assignmentId, interviewId: Id.g2, status: InterviewStatus.RejectedBySupervisor));

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            assignments.Store(Create.Entity.AssignmentDocument(assignmentId).Build());

            var handler =  Create.Service.SupervisorInterviewsHandler(
                interviews: existingInterviews,
                assignments: assignments);
            // Act
            await handler.UploadInterview(new UploadInterviewRequest
            {
                InterviewKey = Create.Entity.InterviewKey().ToString(),
                Interview = new InterviewPackageApiView
                {
                    InterviewId = Id.g2,
                    Events = "",
                    MetaInfo = new InterviewMetaInfo()
                }
            });

            // Assert
            var assignmentDocument = assignments.GetById(assignmentId);
            Assert.That(assignmentDocument, Has.Property(nameof(assignmentDocument.CreatedInterviewsCount)).EqualTo(2));
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

        [Test]
        public async Task when_UploadInterview_and_no_longer_assignment_by_uploaded_interview_should_not_throw_an_exception_when_trying_to_update_interviews_count_by_assignment()
        {
            //Arrange
            var assignmentId = 1;
            var existingInterviews = Create.Storage.SqliteInmemoryStorage(
                Create.Entity.InterviewView(assignmentId: assignmentId, interviewId: Id.g1));

            var assignments = Create.Storage.AssignmentDocumentsInmemoryStorage();
            var mockOfBrokenPackagesStorage = new Mock<IPlainStorage<BrokenInterviewPackageView, int?>>();

            var handler = Create.Service.SupervisorInterviewsHandler(
                interviews: existingInterviews,
                assignments: assignments,
                brokenInterviewStorage: mockOfBrokenPackagesStorage.Object);

            // Act
            await handler.UploadInterview(new UploadInterviewRequest
            {
                InterviewKey = Create.Entity.InterviewKey().ToString(),
                Interview = new InterviewPackageApiView
                {
                    InterviewId = Id.g1,
                    Events = "",
                    MetaInfo = new InterviewMetaInfo()
                }
            });

            //Assert
            mockOfBrokenPackagesStorage.Verify(x => x.Store(It.IsAny<BrokenInterviewPackageView>()), Times.Never);
        }
    }
}

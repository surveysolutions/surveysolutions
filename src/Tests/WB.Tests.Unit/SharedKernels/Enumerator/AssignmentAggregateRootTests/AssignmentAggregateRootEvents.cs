using System;
using System.Collections.Generic;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.AssignmentAggregateRootTests
{
    [TestOf(typeof(AssignmentAggregateRoot))]
    public class AssignmentAggregateRootEvents : AssignmentAggregateRootTestContext
    {
        [Test]
        public void when_create_assignment()
        {
            //arrange
            var assignmentPublicKey = Guid.NewGuid();
            var assignmentId = 7;
            var userId = Guid.NewGuid();
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.NewGuid(), 7);
            var responsibleId = Guid.NewGuid();
            var quantity = 77;
            var isAudioRecordingEnabled = true;
            var email = "email";
            var password = "password";
            var webMode = true;
            var interviewAnswers = new List<InterviewAnswer>()
            {
                new InterviewAnswer()
                {
                    Identity = Create.Identity(Guid.NewGuid(), 1, 2, 3),
                    Answer = TextAnswer.FromString("text")
                },
                new InterviewAnswer()
                {
                    Identity = Create.Identity(Guid.NewGuid(), 1, 2, 3),
                    Answer = NumericIntegerAnswer.FromInt(4)
                },
            };
            var protectedVariables = new List<string>()
            {
                "test1",
                "tess3"
            };
            var comment = " comment";

            var command = Create.Command.CreateAssignment(
                assignmentId: assignmentPublicKey,
                id: assignmentId,
                userId: userId,
                questionnaireId: questionnaireIdentity,
                responsibleId: responsibleId,
                quantity: quantity,
                audioRecording: isAudioRecordingEnabled,
                email: email,
                password: password,
                webMode: webMode,
                answers: interviewAnswers,
                protectedVariables: protectedVariables,
                comment: comment);

            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.CreateAssignment(command);

            //assert
            Assert.That(command.UserId, Is.EqualTo(userId));
            Assert.That(command.PublicKey, Is.EqualTo(assignmentPublicKey));
            Assert.That(assignment.properties.Id, Is.EqualTo(assignmentId));
            Assert.That(assignment.properties.ResponsibleId, Is.EqualTo(responsibleId));
            Assert.That(assignment.properties.AudioRecording, Is.EqualTo(isAudioRecordingEnabled));
            Assert.That(assignment.properties.Comment, Is.EqualTo(comment));
            Assert.That(assignment.properties.CreatedAt, Is.EqualTo(command.OriginDate));
            Assert.That(assignment.properties.UpdatedAt, Is.EqualTo(command.OriginDate));
            Assert.That(assignment.properties.Answers, Is.EqualTo(interviewAnswers));
            Assert.That(assignment.properties.ProtectedVariables, Is.EqualTo(protectedVariables));
            Assert.That(assignment.properties.Email, Is.EqualTo(email));
            Assert.That(assignment.properties.Password, Is.EqualTo(password));
            Assert.That(assignment.properties.WebMode, Is.EqualTo(webMode));
            Assert.That(assignment.properties.Archived, Is.False);
            Assert.That(assignment.properties.IsDeleted, Is.False);
        }

        [Test]
        public void when_created_with_minus_one_as_quantity()
        {
            
            var command = Create.Command.CreateAssignment(
                quantity: -1
                );

            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            using (var eventContext = new EventContext())
            {
                assignment.CreateAssignment(command);
                Assert.That(assignment.properties.Quantity, Is.Null);
                eventContext.ShouldContainEvent<AssignmentCreated>(x => x.Quantity == null);
            }
        }

        [Test]
        public void when_archive_assignment()
        {
            var command = Create.Command.ArchiveAssignment();
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.Archive(command);

            //assert
            Assert.That(assignment.properties.Archived, Is.True);
        }

        [Test]
        public void when_unarchive_assignment()
        {
            var command = Create.Command.UnarchiveAssignment();

            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.Unarchive(command);

            //assert
            Assert.That(assignment.properties.Archived, Is.False);
        }

        [Test]
        public void when_delete_assignment()
        {
            var command = Create.Command.DeleteAssignment();
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.DeleteAssignment(command);

            //assert
            Assert.That(assignment.properties.IsDeleted, Is.True);
        }

        [Test]
        public void when_mark_assignment_received_by_tablet()
        {
            var command = Create.Command.MarkAssignmentAsReceivedByTablet();
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.MarkAssignmentAsReceivedByTablet(command);

            //assert
            Assert.That(assignment.properties.ReceivedByTabletAt, Is.EqualTo(command.OriginDate));
        }

        [Test]
        public void when_reassign_assignment()
        {
            var responsibleId = Guid.NewGuid();
            var command = Create.Command.ReassignAssignment(responsibleId: responsibleId);
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.Reassign(command);

            //assert
            Assert.That(assignment.properties.ResponsibleId, Is.EqualTo(responsibleId));
        }

        [Test]
        public void when_update_audio_recording_assignment()
        {
            var command = Create.Command.UpdateAssignmentAudioRecording(audioRecording: true);
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.UpdateAssignmentAudioRecording(command);

            //assert
            Assert.That(assignment.properties.AudioRecording, Is.True);
        }

        [Test]
        public void when_update_quantity_assignment()
        {
            var command = Create.Command.UpdateAssignmentQuantity(quantity: 7);
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.UpdateAssignmentQuantity(command);

            //assert
            Assert.That(assignment.properties.Quantity, Is.EqualTo(7));
        }

        [Test]
        public void when_update_quantity_received_minus_one_should_publish_null_quantity()
        {
            var command = Create.Command.UpdateAssignmentQuantity(quantity: -1);
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            using (var context = new EventContext())
            {
                assignment.UpdateAssignmentQuantity(command);

                //assert
                Assert.That(assignment.properties.Quantity, Is.Null);
                context.ShouldContainEvent<AssignmentQuantityChanged>(e => e.Quantity == null);
            }
        }
        
        [Test]
        public void when_assignment_target_area_updated_should_publish_new_name()
        {
            var areaName = "area1.shp";
            var command = Create.Command.UpdateAssignmentTargetArea(targetAreaName: areaName);
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            using (var context = new EventContext())
            {
                assignment.UpdateAssignmentTargetArea(command);

                //assert
                Assert.That(assignment.properties.TargetArea, Is.EqualTo(areaName));
                context.ShouldContainEvent<AssignmentTargetAreaChanged>(e => e.TargetAreaName == areaName);
            }
        }

        [Test]
        public void when_update_web_mode_assignment()
        {
            var command = Create.Command.UpdateAssignmentWebMode(webMode: true);
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            assignment.UpdateAssignmentWebMode(command);

            //assert
            Assert.That(assignment.properties.WebMode, Is.True);
        }

        [Test]
        public void when_update_web_mode_assignment_when_assignment_is_deleted()
        {
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();
            assignment.DeleteAssignment(Create.Command.DeleteAssignment());

            var command = Create.Command.UpdateAssignmentWebMode(webMode: true);

            //act
            var exception = Assert.Catch(() => assignment.UpdateAssignmentWebMode(command));

            //assert
            Assert.That(exception, Is.Not.Null);
            var assignmentException = exception as AssignmentException;
            Assert.That(assignmentException, Is.Not.Null);
            Assert.That(assignmentException.ExceptionType, Is.EqualTo(AssignmentDomainExceptionType.AssignmentDeleted));
        }

        [Test]
        public void when_upgrade_assignment_occurs_Should_raise_events()
        {
            var assignment = Create.AggregateRoot.AssignmentAggregateRoot();

            //act
            using (var eventContext = new EventContext())
            {
                assignment.UpgradeAssignment(Create.Command.UpgradeAssignment());

                // Assert
                eventContext.ShouldContainEvent<AssignmentArchived>();
                eventContext.ShouldContainEvent<AssignmentWebModeChanged>(e => e.WebMode == false);
            }
        }
    }
}

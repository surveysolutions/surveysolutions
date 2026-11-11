using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using NHibernate;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(ReminderEmailSender))]
    public class ReminderEmailSenderTests
    {
        [Test]
        public void when_delivery_lock_is_unavailable_then_no_recipient_is_sent()
        {
            var query = new Mock<ISQLQuery>();
            query.Setup(x => x.SetString(It.IsAny<string>(), It.IsAny<string>())).Returns(query.Object);
            query.Setup(x => x.UniqueResultAsync<bool>(It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var session = new Mock<ISession>();
            session.Setup(x => x.CreateSQLQuery(It.IsAny<string>())).Returns(query.Object);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.Session).Returns(session.Object);
            var email = new Mock<IEmailService>();
            var sender = Create.Service.ReminderEmailSender(emailService: email.Object, unitOfWork: unitOfWork.Object);

            Assert.ThrowsAsync<ReminderDeliveryInProgressException>(() => sender.SendRemindersAsync(
                Create.Entity.QuestionnaireIdentity(), "Survey", EmailTextTemplateType.Reminder_NoResponse, 0));
            email.Verify(x => x.GetSenderInfo(), Times.Never);
        }

        [Test]
        public async Task when_delivery_is_locked_then_the_questionnaire_key_is_used()
        {
            var lockKeys = new List<string>();
            var query = new Mock<ISQLQuery>();
            query.Setup(x => x.SetString(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, value) => lockKeys.Add(value))
                .Returns(query.Object);
            query.Setup(x => x.UniqueResultAsync<bool>(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.CreateSQLQuery(It.IsAny<string>())).Returns(query.Object);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.Session).Returns(session.Object);
            var identity = Create.Entity.QuestionnaireIdentity();

            await ReminderEmailSendLock.TryAcquireDeliveryAsync(unitOfWork.Object, identity);
            await ReminderEmailSendLock.TryAcquireDeliveryAsync(unitOfWork.Object, identity);

            Assert.That(lockKeys, Has.Count.EqualTo(2));
            Assert.That(lockKeys[0], Is.EqualTo(lockKeys[1]));
            Assert.That(lockKeys[0], Does.Contain(identity.ToString()));
        }

        [TestCase(EmailTextTemplateType.Reminder_NoResponse)]
        [TestCase(EmailTextTemplateType.Reminder_PartialResponse)]
        [TestCase(EmailTextTemplateType.RejectEmail)]
        public async Task when_recovering_request_then_successes_and_failures_are_retained_without_resending(
            EmailTextTemplateType type)
        {
            var delivery = new DeliveryContext();
            var requestId = Guid.NewGuid();
            delivery.Email.SetupSequence(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("email-id")
                .ThrowsAsync(new InvalidOperationException("Provider failed"));
            delivery.SetRecipients(type, 1, 2);

            var first = await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey", type, 0,
                requestId: requestId);
            Assert.That(first.SentCount, Is.EqualTo(1));
            Assert.That(first.FailedCount, Is.Zero);
            Assert.That(first.UnconfirmedCount, Is.EqualTo(1));
            // The successful recipient can disappear from the eligibility query after a restart.
            delivery.SetRecipients(type, 2);
            var recovered = await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey", type, 0,
                requestId: requestId);

            Assert.That(recovered.SentCount, Is.EqualTo(1));
            Assert.That(recovered.FailedCount, Is.Zero);
            Assert.That(recovered.UnconfirmedCount, Is.EqualTo(1));
            Assert.That(delivery.Checkpoints.GetById(
                ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId, 1)).Sent, Is.True);
            Assert.That(delivery.Checkpoints.GetById(
                ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId, 2)).Unconfirmed, Is.True);
            delivery.Email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Test]
        public async Task when_current_web_configuration_is_stopped_then_cached_started_configuration_is_not_used()
        {
            var delivery = new DeliveryContext();
            delivery.Configs.Setup(x => x.GetById(It.IsAny<string>())).Returns(new WebInterviewConfig { Started = true });
            delivery.Configs.Setup(x => x.GetByIdUncached(It.IsAny<string>()))
                .Returns(new WebInterviewConfig { Started = false });
            delivery.SetRecipients(EmailTextTemplateType.Reminder_NoResponse, 1);

            var result = await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey",
                EmailTextTemplateType.Reminder_NoResponse, 0);

            Assert.That(result.SentCount, Is.Zero);
            delivery.Configs.Verify(x => x.GetById(It.IsAny<string>()), Times.Never);
            delivery.Email.Verify(x => x.GetSenderInfo(), Times.Never);
        }

        [TestCase(EmailTextTemplateType.Reminder_NoResponse)]
        [TestCase(EmailTextTemplateType.RejectEmail)]
        public async Task when_sending_now_then_delivery_attempt_is_committed_before_dispatch(
            EmailTextTemplateType type)
        {
            var delivery = new DeliveryContext();
            var requestId = Guid.NewGuid();
            var scopedParameters = new TestInMemoryKeyValueStorage<EmailParameters>();
            var locator = new Mock<IServiceLocator>();
            locator.Setup(x => x.GetInstance<IInvitationService>()).Returns(delivery.Invitations.Object);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<EmailParameters>>()).Returns(scopedParameters);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>())
                .Returns(delivery.Outcomes);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryInvitationChunk>>())
                .Returns(delivery.InvitationChunks);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>())
                .Returns(delivery.Checkpoints);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>())
                .Returns(delivery.UnresolvedDeliveries);
            var executor = new Mock<IInScopeExecutor>();
            var scopeCount = 0;
            executor.Setup(x => x.ExecuteAsync(It.IsAny<Func<IServiceLocator, Task>>(), It.IsAny<string>()))
                .Returns(async (Func<IServiceLocator, Task> action, string _) =>
                {
                    scopeCount++;
                    await action(locator.Object);
                    Assert.That(scopedParameters.Storage.Count, Is.EqualTo(1));
                    var checkpoint = delivery.Checkpoints.GetById(
                        ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId, 1));
                    if (scopeCount == 1)
                    {
                        Assert.That(delivery.InvitationChunks.GetById(
                            ReminderEmailDeliveryInvitationChunk.GetStorageKey(requestId, 0)).InvitationIds,
                            Does.Contain(1));
                        Assert.That(checkpoint.AttemptInProgress, Is.True);
                        delivery.Email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
                    }
                    else
                    {
                        Assert.That(delivery.Outcomes.GetById(ReminderEmailDeliveryOutcomes.GetStorageKey(requestId))
                            .SentCount, Is.EqualTo(1));
                        Assert.That(checkpoint.Sent, Is.True);
                        Assert.That(checkpoint.AttemptInProgress, Is.False);
                    }
                });
            var rootParameters = new Mock<IPlainKeyValueStorage<EmailParameters>>();
            var sender = delivery.CreateSender(executor.Object, rootParameters.Object);
            delivery.SetRecipients(type, 1);

            await sender.SendRemindersAsync(delivery.Identity, "Survey", type, 0, requestId: requestId);

            Assert.That(scopeCount, Is.EqualTo(2));
            rootParameters.Verify(x => x.Store(It.IsAny<EmailParameters>(), It.IsAny<string>()), Times.Never);
            if (type == EmailTextTemplateType.RejectEmail)
                delivery.Invitations.Verify(x => x.MarkRejectedInterviewReminderSent(1, "email-id"), Times.Once);
            else
                delivery.Invitations.Verify(x => x.MarkInvitationAsReminded(1, "email-id"), Times.Once);
        }

        [Test]
        public async Task when_request_is_interrupted_after_provider_accepts_then_recipient_is_not_resent()
        {
            var delivery = new DeliveryContext();
            var requestId = Guid.NewGuid();
            delivery.SetRecipients(EmailTextTemplateType.Reminder_NoResponse, 1);
            delivery.Invitations.Setup(x => x.MarkInvitationAsReminded(1, "email-id"))
                .Throws(new InvalidOperationException("Delivery state could not be committed."));

            var sender = delivery.Sender;
            Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendRemindersAsync(delivery.Identity,
                "Survey", EmailTextTemplateType.Reminder_NoResponse, 0, requestId));
            delivery.SetRecipients(EmailTextTemplateType.Reminder_NoResponse);

            var recovered = await sender.SendRemindersAsync(delivery.Identity, "Survey",
                EmailTextTemplateType.Reminder_NoResponse, 0, requestId);

            Assert.That(recovered.SentCount, Is.Zero);
            Assert.That(recovered.FailedCount, Is.Zero);
            Assert.That(recovered.UnconfirmedCount, Is.EqualTo(1));
            Assert.That(delivery.Outcomes.GetById(ReminderEmailDeliveryOutcomes.GetStorageKey(requestId))
                .UnconfirmedCount, Is.EqualTo(1));
            Assert.That(delivery.Checkpoints.GetById(
                ReminderEmailDeliveryCheckpoint.GetStorageKey(requestId, 1)).Unconfirmed, Is.True);
            Assert.That(delivery.UnresolvedDeliveries.GetById(ReminderEmailUnresolvedDelivery.GetStorageKey(
                delivery.Identity, 1)), Is.Not.Null);
            delivery.Email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task when_delivery_outcome_is_unconfirmed_then_other_reminder_types_skip_the_invitation()
        {
            var delivery = new DeliveryContext();
            delivery.Email.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Provider timed out"));
            delivery.SetRecipients(EmailTextTemplateType.Reminder_NoResponse, 1);
            delivery.SetRecipients(EmailTextTemplateType.Reminder_PartialResponse, 1);

            var firstResult = await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey",
                EmailTextTemplateType.Reminder_NoResponse, 0);
            var secondResult = await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey",
                EmailTextTemplateType.Reminder_PartialResponse, 0);

            Assert.That(firstResult.UnconfirmedCount, Is.EqualTo(1));
            Assert.That(secondResult.SentCount, Is.Zero);
            Assert.That(secondResult.FailedCount, Is.Zero);
            delivery.Email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private class DeliveryContext
        {
            public readonly QuestionnaireIdentity Identity = Create.Entity.QuestionnaireIdentity(Id.g1, 1);
            public readonly Mock<IInvitationService> Invitations = new Mock<IInvitationService>();
            public readonly Mock<IEmailService> Email = new Mock<IEmailService>();
            public readonly Mock<IWebInterviewEmailRenderer> Renderer = new Mock<IWebInterviewEmailRenderer>();
            public readonly Mock<IPlainKeyValueStorage<WebInterviewConfig>> Configs =
                new Mock<IPlainKeyValueStorage<WebInterviewConfig>>();
            public readonly TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes> Outcomes =
                new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            public readonly TestInMemoryKeyValueStorage<ReminderEmailDeliveryInvitationChunk> InvitationChunks =
                new TestInMemoryKeyValueStorage<ReminderEmailDeliveryInvitationChunk>();
            public readonly TestInMemoryKeyValueStorage<ReminderEmailDeliveryCheckpoint> Checkpoints =
                new TestInMemoryKeyValueStorage<ReminderEmailDeliveryCheckpoint>();
            public readonly TestInMemoryKeyValueStorage<ReminderEmailUnresolvedDelivery> UnresolvedDeliveries =
                new TestInMemoryKeyValueStorage<ReminderEmailUnresolvedDelivery>();
            public readonly Invitation Invitation = Create.Entity.Invitation(1,
                Create.Entity.Assignment(email: "respondent@email.com"), token: "token");

            public DeliveryContext()
            {
                Invitations.Setup(x => x.GetInvitation(1)).Returns(Invitation);
                Invitations.Setup(x => x.GetInvitation(2)).Returns(Create.Entity.Invitation(2,
                    Create.Entity.Assignment(email: "second@email.com"), token: "second"));
                Configs.Setup(x => x.GetByIdUncached(It.IsAny<string>()))
                    .Returns(new WebInterviewConfig { Started = true });
                Email.Setup(x => x.GetSenderInfo()).Returns(Mock.Of<ISenderInformation>());
                Email.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("email-id");
                Renderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
                    .ReturnsAsync(new PersonalizedWebInterviewEmail("subject", "html", "text"));
            }

            [Test]
            public async Task when_request_is_interrupted_after_delivery_then_recovery_keeps_committed_counts()
            {
                var delivery = new DeliveryContext();
                var requestId = Guid.NewGuid();
                delivery.SetRecipients(EmailTextTemplateType.Reminder_NoResponse, 1, 2);
                delivery.Renderer.SetupSequence(x => x.RenderEmail(It.IsAny<EmailParams>()))
                    .ReturnsAsync(new PersonalizedWebInterviewEmail("subject", "html", "text"))
                    .ThrowsAsync(new InvalidOperationException("Rendering failed"));

                Assert.ThrowsAsync<InvalidOperationException>(() => delivery.Sender.SendRemindersAsync(delivery.Identity,
                    "Survey", EmailTextTemplateType.Reminder_NoResponse, 0, requestId: requestId));
                delivery.Renderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
                    .ReturnsAsync(new PersonalizedWebInterviewEmail("subject", "html", "text"));

                var recovered = await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey",
                    EmailTextTemplateType.Reminder_NoResponse, 0, requestId: requestId);

                Assert.That(recovered.SentCount, Is.EqualTo(2));
                Assert.That(recovered.FailedCount, Is.Zero);
                delivery.Email.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            }

            public ReminderEmailSender Sender => CreateSender();

            public ReminderEmailSender CreateSender(IInScopeExecutor executor = null,
                IPlainKeyValueStorage<EmailParameters> parameters = null) =>
                Create.Service.ReminderEmailSender(invitationService: Invitations.Object, emailService: Email.Object,
                    webInterviewEmailRenderer: Renderer.Object, webInterviewConfigs: Configs.Object,
                    deliveryOutcomes: Outcomes, deliveryCheckpoints: Checkpoints,
                    deliveryInvitationChunks: InvitationChunks, unresolvedDeliveries: UnresolvedDeliveries,
                    inScopeExecutor: executor, emailParamsStorage: parameters);

            public void SetRecipients(EmailTextTemplateType type, params int[] ids)
            {
                if (type == EmailTextTemplateType.Reminder_NoResponse)
                    Invitations.Setup(x => x.GetNoResponseInvitations(Identity, 0)).Returns(ids);
                else if (type == EmailTextTemplateType.Reminder_PartialResponse)
                    Invitations.Setup(x => x.GetPartialResponseInvitations(Identity, 0)).Returns(ids);
                else
                    Invitations.Setup(x => x.GetInvitationsWithRejectedInterview(Identity)).Returns(ids);
            }
        }

        [TestCase(EmailTextTemplateType.Reminder_NoResponse)]
        [TestCase(EmailTextTemplateType.Reminder_PartialResponse)]
        public async Task when_sending_now_and_reminder_is_disabled(EmailTextTemplateType reminderType)
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var invitationService = new Mock<IInvitationService>();
            invitationService.SetReturnsDefault<IEnumerable<int>>(Enumerable.Empty<int>());

            var config = new WebInterviewConfig { Started = true };
            var configProvider = new Mock<IWebInterviewConfigProvider>();
            configProvider.Setup(x => x.Get(questionnaireIdentity)).Returns(config);

            var sender = Create.Service.ReminderEmailSender(
                invitationService: invitationService.Object,
                webInterviewConfigProvider: configProvider.Object);

            await sender.SendRemindersAsync(questionnaireIdentity, "Web Survey", reminderType, 0);

            if (reminderType == EmailTextTemplateType.Reminder_NoResponse)
                invitationService.Verify(x => x.GetNoResponseInvitations(questionnaireIdentity, 0), Times.Once);
            else
                invitationService.Verify(x => x.GetPartialResponseInvitations(questionnaireIdentity, 0), Times.Once);
        }

        [Test]
        public async Task when_provider_outcome_is_ambiguous_then_result_is_unconfirmed_and_claim_is_retained()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var invitation = Create.Entity.Invitation(1,
                Create.Entity.Assignment(email: "respondent@email.com", password: "password"),
                token: "token");
            var invitationService = new Mock<IInvitationService>();
            invitationService.Setup(x => x.GetNoResponseInvitations(questionnaireIdentity, 0))
                .Returns(new[] { invitation.Id });
            invitationService.Setup(x => x.GetInvitation(invitation.Id)).Returns(invitation);

            var emailService = new Mock<IEmailService>();
            emailService.Setup(x => x.GetSenderInfo()).Returns(Mock.Of<ISenderInformation>());
            emailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Provider timed out"));

            var configProvider = new Mock<IWebInterviewConfigProvider>();
            configProvider.Setup(x => x.Get(questionnaireIdentity)).Returns(new WebInterviewConfig { Started = true });
            var emailRenderer = new Mock<IWebInterviewEmailRenderer>();
            emailRenderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
                .ReturnsAsync(new PersonalizedWebInterviewEmail("subject", "html", "text"));
            var emailParamsStorage = new Mock<IPlainKeyValueStorage<EmailParameters>>();
            var outcomes = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            var invitationChunks = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryInvitationChunk>();
            var checkpoints = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryCheckpoint>();
            var unresolvedDeliveries = new TestInMemoryKeyValueStorage<ReminderEmailUnresolvedDelivery>();
            var scopedServiceLocator = new Mock<IServiceLocator>();
            scopedServiceLocator.Setup(x => x.GetInstance<IInvitationService>()).Returns(invitationService.Object);
            scopedServiceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<EmailParameters>>())
                .Returns(emailParamsStorage.Object);
            scopedServiceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>())
                .Returns(outcomes);
            scopedServiceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryInvitationChunk>>())
                .Returns(invitationChunks);
            scopedServiceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>())
                .Returns(checkpoints);
            scopedServiceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>())
                .Returns(unresolvedDeliveries);
            var inScopeExecutor = new Mock<IInScopeExecutor>();
            inScopeExecutor.Setup(x => x.ExecuteAsync(It.IsAny<Func<IServiceLocator, Task>>(), It.IsAny<string>()))
                .Returns((Func<IServiceLocator, Task> action, string _) => action(scopedServiceLocator.Object));

            var sender = Create.Service.ReminderEmailSender(
                invitationService: invitationService.Object,
                emailService: emailService.Object,
                webInterviewConfigProvider: configProvider.Object,
                webInterviewEmailRenderer: emailRenderer.Object,
                emailParamsStorage: emailParamsStorage.Object,
                inScopeExecutor: inScopeExecutor.Object);

            var result = await sender.SendRemindersAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, 0);

            Assert.That(result.SentCount, Is.Zero);
            Assert.That(result.FailedCount, Is.Zero);
            Assert.That(result.UnconfirmedCount, Is.EqualTo(1));
            invitationService.Verify(x => x.ReminderWasNotSent(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.That(unresolvedDeliveries.GetById(ReminderEmailUnresolvedDelivery.GetStorageKey(
                questionnaireIdentity, invitation.Id)), Is.Not.Null);
            emailParamsStorage.Verify(x => x.Store(It.IsAny<EmailParameters>(), It.IsAny<string>()), Times.Once);
            inScopeExecutor.Verify(x => x.ExecuteAsync(It.IsAny<Func<IServiceLocator, Task>>(), It.IsAny<string>()),
                Times.Exactly(2));
        }

        [Test]
        public async Task when_provider_definitively_rejects_delivery_then_result_reports_failure()
        {
            var delivery = new DeliveryContext();
            delivery.Email.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new EmailServiceException("recipient@email.com", System.Net.HttpStatusCode.BadRequest));
            delivery.SetRecipients(EmailTextTemplateType.Reminder_NoResponse, 1);

            var result = await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey",
                EmailTextTemplateType.Reminder_NoResponse, 0);

            Assert.That(result.FailedCount, Is.EqualTo(1));
            Assert.That(result.UnconfirmedCount, Is.Zero);
            delivery.Invitations.Verify(x => x.ReminderWasNotSent(1, delivery.Invitation.AssignmentId,
                delivery.Invitation.Assignment.Email, It.IsAny<string>()), Times.Once);
            Assert.That(delivery.UnresolvedDeliveries.GetById(ReminderEmailUnresolvedDelivery.GetStorageKey(
                delivery.Identity, 1)), Is.Null);
        }

        [Test]
        public async Task when_delivery_index_grows_then_recipient_ids_are_stored_in_bounded_chunks()
        {
            var delivery = new DeliveryContext();
            var requestId = Guid.NewGuid();
            delivery.SetRecipients(EmailTextTemplateType.Reminder_NoResponse, Enumerable.Range(1, 101).ToArray());
            delivery.Invitations.Setup(x => x.GetInvitation(It.IsAny<int>()))
                .Returns((int id) => Create.Entity.Invitation(id,
                    Create.Entity.Assignment(email: $"recipient-{id}@email.com"), token: "token"));

            await delivery.Sender.SendRemindersAsync(delivery.Identity, "Survey",
                EmailTextTemplateType.Reminder_NoResponse, 0, requestId);

            var outcomes = delivery.Outcomes.GetById(ReminderEmailDeliveryOutcomes.GetStorageKey(requestId));
            Assert.That(outcomes.InvitationCount, Is.EqualTo(101));
            Assert.That(outcomes.InvitationChunkCount, Is.EqualTo(2));
            Assert.That(delivery.InvitationChunks.GetById(
                ReminderEmailDeliveryInvitationChunk.GetStorageKey(requestId, 0)).InvitationIds, Has.Count.EqualTo(100));
            Assert.That(delivery.InvitationChunks.GetById(
                ReminderEmailDeliveryInvitationChunk.GetStorageKey(requestId, 1)).InvitationIds, Has.Count.EqualTo(1));
        }

    }

    [TestOf(typeof(SendRemindersJob))]
    public class SendRemindersJobTests
    {
        [Test]
        public async Task when_delivery_is_already_in_progress_then_scheduled_job_skips_batch_and_continues()
        {
            var identity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var invitations = new Mock<IInvitationService>();
            invitations.Setup(x => x.GetQuestionnairesWithInvitations())
                .Returns(new[] { Create.Entity.QuestionnaireBrowseItem(Id.g1, 10, title: "Survey") });
            var config = new Mock<IWebInterviewConfigProvider>();
            config.Setup(x => x.Get(identity)).Returns(new WebInterviewConfig
            {
                Started = true, ReminderAfterDaysIfNoResponse = 1, ReminderAfterDaysIfPartialResponse = 1
            });
            var email = new Mock<IEmailService>();
            email.Setup(x => x.IsConfigured()).Returns(true);
            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(identity, "Survey", It.IsAny<EmailTextTemplateType>(),
                It.IsAny<int>())).ReturnsAsync(new ReminderEmailSendResult());
            sender.Setup(x => x.SendRemindersAsync(identity, "Survey",
                EmailTextTemplateType.Reminder_NoResponse, 1)).ThrowsAsync(new ReminderDeliveryInProgressException());
            var locator = new Mock<IServiceLocator>();
            locator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var job = new SendRemindersJob(Mock.Of<ILogger<SendRemindersJob>>(), invitations.Object,
                email.Object, config.Object, new NoScopeInScopeExecutor(locator.Object));

            await job.Execute(Mock.Of<IJobExecutionContext>());

            sender.Verify(x => x.SendRemindersAsync(identity, "Survey",
                EmailTextTemplateType.Reminder_PartialResponse, 1), Times.Once);
            sender.Verify(x => x.SendRemindersAsync(identity, "Survey", EmailTextTemplateType.RejectEmail, 0), Times.Once);
        }

        [Test]
        public async Task when_all_reminder_types_are_enabled_then_each_batch_uses_a_separate_scope()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var invitationService = new Mock<IInvitationService>();
            invitationService.Setup(x => x.GetQuestionnairesWithInvitations())
                .Returns(new[] { Create.Entity.QuestionnaireBrowseItem(Id.g1, 10, title: "Web Survey") });
            var configProvider = new Mock<IWebInterviewConfigProvider>();
            configProvider.Setup(x => x.Get(questionnaireIdentity)).Returns(new WebInterviewConfig
            {
                Started = true,
                ReminderAfterDaysIfNoResponse = 1,
                ReminderAfterDaysIfPartialResponse = 1
            });
            var emailService = new Mock<IEmailService>();
            emailService.Setup(x => x.IsConfigured()).Returns(true);
            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(questionnaireIdentity, "Web Survey",
                    It.IsAny<EmailTextTemplateType>(), It.IsAny<int>()))
                .ReturnsAsync(new ReminderEmailSendResult());
            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var rootScopeExecutor = new Mock<IRootScopeExecutor>();
            rootScopeExecutor.Setup(x => x.ExecuteAsync<ReminderEmailSendResult>(
                    It.IsAny<Func<IServiceLocator, Task<ReminderEmailSendResult>>>()))
                .Returns((Func<IServiceLocator, Task<ReminderEmailSendResult>> action) =>
                    action(serviceLocator.Object));

            var job = new SendRemindersJob(Mock.Of<ILogger<SendRemindersJob>>(), invitationService.Object,
                emailService.Object, configProvider.Object, rootScopeExecutor.Object);

            await job.Execute(Mock.Of<IJobExecutionContext>());

            rootScopeExecutor.Verify(x => x.ExecuteAsync<ReminderEmailSendResult>(
                It.IsAny<Func<IServiceLocator, Task<ReminderEmailSendResult>>>()), Times.Exactly(3));
        }

        [TestCase(SendRemindersNowState.Completed)]
        [TestCase(SendRemindersNowState.Failed)]
        [TestCase(SendRemindersNowState.PartiallyFailed)]
        public async Task when_recovering_a_terminal_request_then_delivery_is_not_repeated(
            SendRemindersNowState terminalState)
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10),
                QuestionnaireTitle = "Web Survey",
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var storage = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var outcomes = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            storage.Store(new SendRemindersNowStatus
            {
                RequestId = request.RequestId,
                QuestionnaireIdentity = request.QuestionnaireIdentity,
                Type = request.Type,
                State = terminalState
            }, request.StatusStorageKey);

            var sender = new Mock<IReminderEmailSender>();
            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>())
                .Returns(storage);
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>())
                .Returns(outcomes);
            serviceLocator.Setup(x => x.GetInstance<IUnitOfWork>())
                .Returns(SendRemindersNowJobTests.CreateExecutionUnitOfWork());
            serviceLocator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var rootScopeExecutor = new Mock<IRootScopeExecutor>();
            rootScopeExecutor.Setup(x => x.ExecuteAsync<SendRemindersNowStatus>(
                    It.IsAny<Func<IServiceLocator, Task<SendRemindersNowStatus>>>()))
                .Returns((Func<IServiceLocator, Task<SendRemindersNowStatus>> action) =>
                    action(serviceLocator.Object));
            rootScopeExecutor.Setup(x => x.ExecuteAsync<bool>(It.IsAny<Func<IServiceLocator, Task<bool>>>()))
                .Returns((Func<IServiceLocator, Task<bool>> action) => action(serviceLocator.Object));

            var job = new SendRemindersNowJob(rootScopeExecutor.Object, Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            sender.Verify(x => x.SendRemindersAsync(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>(),
                It.IsAny<EmailTextTemplateType>(), It.IsAny<int>()), Times.Never);
            rootScopeExecutor.Verify(x => x.ExecuteAsync<SendRemindersNowStatus>(
                It.IsAny<Func<IServiceLocator, Task<SendRemindersNowStatus>>>()), Times.Once);
        }

        [Test]
        public async Task when_email_service_is_not_configured()
        {
            var invitationServiceMock = new Mock<IInvitationService>();
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.IsConfigured()).Returns(false);

            //arrange 
            var job = Create.Service.SendRemindersJob(invitationService: invitationServiceMock.Object, emailService: emailServiceMock.Object);

            //act
            await job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            invitationServiceMock.Verify(x => x.GetQuestionnairesWithInvitations(), Times.Never);
        }

        [Test]
        public async Task when_reminders_are_turned_off()
        {
            var invitationServiceMock = new Mock<IInvitationService>();
            invitationServiceMock.WithQuestionnaires(Create.Entity.QuestionnaireBrowseItem(Id.g1, 10), Create.Entity.QuestionnaireBrowseItem(Id.g2, 2));
            var settingsMock = new Mock<IWebInterviewConfigProvider>();
            var settings = Mock.Of<WebInterviewConfig>(_ => _.ReminderAfterDaysIfNoResponse == null && _.ReminderAfterDaysIfPartialResponse == null);
            settingsMock.Setup(x => x.Get(It.IsAny<QuestionnaireIdentity>())).Returns(settings);

            //arrange 
            var job = Create.Service.SendRemindersJob(
                invitationService: invitationServiceMock.Object, 
                webInterviewConfigProvider: settingsMock.Object);

            //act
            await job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            invitationServiceMock.Verify(x => x.GetNoResponseInvitations(Create.Entity.QuestionnaireIdentity(Id.g1, 10), It.IsAny<int>()), Times.Never);
            invitationServiceMock.Verify(x => x.GetPartialResponseInvitations(Create.Entity.QuestionnaireIdentity(Id.g2, 2), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task when_web_mode_is_turned_off()
        {
            var invitationServiceMock = new Mock<IInvitationService>();
            invitationServiceMock.WithQuestionnaires(Create.Entity.QuestionnaireBrowseItem(Id.g1, 10), Create.Entity.QuestionnaireBrowseItem(Id.g2, 2));
            var settingsMock = new Mock<IWebInterviewConfigProvider>();
            var settings = Mock.Of<WebInterviewConfig>(_ => 
                _.Started == false &&
                _.ReminderAfterDaysIfPartialResponse == 2 &&
                _.GetEmailTemplate(EmailTextTemplateType.Reminder_PartialResponse) == Create.Entity.EmailTemplate("Subject: %SURVEYNAME%", "%SURVEYNAME% %PASSWORD% %SURVEYLINK%", "", ""));

            settingsMock.Setup(x => x.Get(It.IsAny<QuestionnaireIdentity>())).Returns(settings);

            //arrange 
            var job = Create.Service.SendRemindersJob(
                invitationService: invitationServiceMock.Object, 
                webInterviewConfigProvider: settingsMock.Object);

            //act
            await job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            invitationServiceMock.Verify(x => x.GetNoResponseInvitations(Create.Entity.QuestionnaireIdentity(Id.g1, 10), It.IsAny<int>()), Times.Never);
            invitationServiceMock.Verify(x => x.GetPartialResponseInvitations(Create.Entity.QuestionnaireIdentity(Id.g2, 2), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task when_sending_partial_response_reminders()
        {
            //arrange 
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);

            List<SentEmailForTests> sentEmails = new List<SentEmailForTests>();
            var emailParameters = new TestInMemoryKeyValueStorage<EmailParameters>();

            var emailService = CreateEmailService()
                .WithSenderInfo()
                .CollectSentEmails(sentEmails, new [] {"email_id_1", "email_id_2"});

            var invitationServiceMock = CreateInvitationService()
                .WithQuestionnaires(Create.Entity.QuestionnaireBrowseItem(Id.g1, 10, title: "Web Survey"))
                .WithPartialInvitations(questionnaireIdentity, 2, 4)
                .WithInvitations(
                    Create.Entity.Invitation(2, Create.Entity.Assignment(email: "one@email.com", password: "AAAAAAAA"), token: "token1"), 
                    Create.Entity.Invitation(4, Create.Entity.Assignment(email: "two@email.com", password: "BBBBBBBB"), token: "token2"));
            
            var settingsMock = new Mock<IWebInterviewConfigProvider>();
            var settings = Mock.Of<WebInterviewConfig>(_ => 
                _.Started == true &&
                _.ReminderAfterDaysIfPartialResponse == 2 &&
                _.GetEmailTemplate(EmailTextTemplateType.Reminder_PartialResponse) == Create.Entity.EmailTemplate("Subject: %SURVEYNAME%", "%SURVEYNAME% %PASSWORD% %SURVEYLINK%", "", ""));

            settingsMock.Setup(x => x.Get(questionnaireIdentity)).Returns(settings);

            var personalizedWebInterviewEmail = new PersonalizedWebInterviewEmail("subject", "html", "text");
            var paramsIds = new List<string>();
            var webInterviewEmailRenderer = new Mock<IWebInterviewEmailRenderer>();
            webInterviewEmailRenderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
                .ReturnsAsync(personalizedWebInterviewEmail)
                .Callback<EmailParams>(x => paramsIds.Add(x.Id));

            var job = Create.Service.SendRemindersJob(
                invitationService: invitationServiceMock.Object, 
                webInterviewConfigProvider: settingsMock.Object,
                emailService: emailService.Object,
                emailParamsStorage: emailParameters,
                webInterviewEmailRenderer: webInterviewEmailRenderer.Object);

            //act
            await job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            Assert.That(sentEmails.Count, Is.EqualTo(2));
            
            Assert.That(sentEmails[0].Email, Is.EqualTo("one@email.com"));
            var paramsId = paramsIds[0];
            Assert.That(emailParameters.GetById(paramsId).Password, Is.EqualTo("AAAAAAAA"));
            Assert.That(emailParameters.GetById(paramsId).SurveyName, Is.EqualTo("Web Survey"));
            Assert.That(emailParameters.GetById(paramsId).Link, Is.EqualTo("http://localhost/WebInterview/token1/Start"));

            Assert.That(sentEmails[1].Email, Is.EqualTo("two@email.com"));
            paramsId = paramsIds[1];
            Assert.That(emailParameters.GetById(paramsId).Password, Is.EqualTo("BBBBBBBB"));
            Assert.That(emailParameters.GetById(paramsId).SurveyName, Is.EqualTo("Web Survey"));
            Assert.That(emailParameters.GetById(paramsId).Link, Is.EqualTo("http://localhost/WebInterview/token2/Start"));
           
            invitationServiceMock.Verify(x => x.MarkInvitationAsReminded(2, "email_id_1"), Times.Once);
            invitationServiceMock.Verify(x => x.MarkInvitationAsReminded(4, "email_id_2"), Times.Once);
        }

        [Test]
        public async Task when_sending_no_response_reminders()
        {
            //arrange 
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g2, 2);

            List<SentEmailForTests> sentEmails = new List<SentEmailForTests>();

            var emailService = CreateEmailService()
                .WithSenderInfo()
                .CollectSentEmails(sentEmails, new [] {"email_id_1"});

            var invitationServiceMock = CreateInvitationService()
                .WithQuestionnaires(Create.Entity.QuestionnaireBrowseItem(Id.g2, 2, title: "Web Survey"))
                .WithNoResponseInvitations(questionnaireIdentity, 1)
                .WithInvitations(Create.Entity.Invitation(1, Create.Entity.Assignment(email: "some@email.com", password: "AAAAAAAA"), token: "token"));
            
            var emailParameters = new TestInMemoryKeyValueStorage<EmailParameters>();

            var settingsMock = new Mock<IWebInterviewConfigProvider>();
            var settings = Mock.Of<WebInterviewConfig>(_ => 
                _.Started == true &&
                _.ReminderAfterDaysIfNoResponse == 2 && 
                _.GetEmailTemplate(EmailTextTemplateType.Reminder_NoResponse) == Create.Entity.EmailTemplate("Subject: %SURVEYNAME%", "%SURVEYNAME% %PASSWORD% %SURVEYLINK%", "", ""));

            settingsMock.Setup(x => x.Get(questionnaireIdentity)).Returns(settings);

            var personalizedWebInterviewEmail = new PersonalizedWebInterviewEmail("subject", "html", "text");
            var webInterviewEmailRenderer = new Mock<IWebInterviewEmailRenderer>();
            webInterviewEmailRenderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
                .ReturnsAsync(personalizedWebInterviewEmail);

            var job = Create.Service.SendRemindersJob(
                invitationService: invitationServiceMock.Object, 
                webInterviewConfigProvider: settingsMock.Object,
                emailService: emailService.Object,
                emailParamsStorage: emailParameters,
                webInterviewEmailRenderer: webInterviewEmailRenderer.Object);

            //act
            await job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            Assert.That(sentEmails.Count, Is.EqualTo(1));
            Assert.That(sentEmails[0].Email, Is.EqualTo("some@email.com"));
            Assert.That(emailParameters.ValueAt(0).Password, Is.EqualTo("AAAAAAAA"));
            Assert.That(emailParameters.ValueAt(0).SurveyName, Is.EqualTo("Web Survey"));
            Assert.That(emailParameters.ValueAt(0).Link, Is.EqualTo("http://localhost/WebInterview/token/Start"));
           
            invitationServiceMock.Verify(x => x.MarkInvitationAsReminded(1, "email_id_1"), Times.Once);
        }

        [Test]
        public async Task when_sending_message_about_rejected_interview()
        {
            //arrange 
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g2, 2);

            List<SentEmailForTests> sentEmails = new List<SentEmailForTests>();

            var emailService = CreateEmailService()
                .WithSenderInfo()
                .CollectSentEmails(sentEmails, new [] {"email_id_1"});

            var invitation = Create.Entity.Invitation(1, 
                Create.Entity.Assignment(email: "some@email.com", password: "AAAAAAAA"),
                interview: Create.Entity.InterviewSummary(status: InterviewStatus.RejectedBySupervisor), 
                token: "token");
            var invitationWithBlankEmail = Create.Entity.Invitation(2,
                Create.Entity.Assignment(email: "   ", password: "BBBBBBBB"),
                interview: Create.Entity.InterviewSummary(status: InterviewStatus.RejectedBySupervisor),
                token: "token2");
            var invitationServiceMock = CreateInvitationService()
                .WithQuestionnaires(Create.Entity.QuestionnaireBrowseItem(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, title: "Web Survey"))
                .WithInvitations(invitation, invitationWithBlankEmail);

            invitationServiceMock.Setup(x => x.GetInvitationsWithRejectedInterview(questionnaireIdentity))
                .Returns(new[] { invitation.Id, invitationWithBlankEmail.Id });
            
            var emailParameters = new TestInMemoryKeyValueStorage<EmailParameters>();

            var personalizedWebInterviewEmail = new PersonalizedWebInterviewEmail("subject", "html", "text");
            var webInterviewEmailRenderer = new Mock<IWebInterviewEmailRenderer>();
            webInterviewEmailRenderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
                .ReturnsAsync(personalizedWebInterviewEmail);

            
            var settings = Mock.Of<WebInterviewConfig>(_ => 
                _.Started == true &&
                _.GetEmailTemplate(EmailTextTemplateType.RejectEmail) == Create.Entity.EmailTemplate("Subject: %SURVEYNAME%", "%SURVEYNAME% %PASSWORD% %SURVEYLINK%", "", ""));
            var settingsMock = new Mock<IWebInterviewConfigProvider>();
            settingsMock.Setup(x => x.Get(questionnaireIdentity)).Returns(settings);
            
            var job = Create.Service.SendRemindersJob(
                invitationService: invitationServiceMock.Object, 
                emailService: emailService.Object,
                emailParamsStorage: emailParameters,
                webInterviewEmailRenderer: webInterviewEmailRenderer.Object,
                webInterviewConfigProvider: settingsMock.Object);

            //act
            await job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            Assert.That(sentEmails.Count, Is.EqualTo(1));
            Assert.That(sentEmails[0].Email, Is.EqualTo("some@email.com"));
            Assert.That(emailParameters.ValueAt(0).Password, Is.EqualTo("AAAAAAAA"));
            Assert.That(emailParameters.ValueAt(0).SurveyName, Is.EqualTo("Web Survey"));
            Assert.That(emailParameters.ValueAt(0).Link, Is.EqualTo("http://localhost/WebInterview/Continue/token"));
           
            invitationServiceMock.Verify(x => x.MarkRejectedInterviewReminderSent(1, "email_id_1"), Times.Once);
        }
        
        private Mock<IInvitationService> CreateInvitationService()
        {
            var invitationService = new Mock<IInvitationService>();
            invitationService.SetReturnsDefault<IEnumerable<int>>(Enumerable.Empty<int>());
            return invitationService;
        }

        private Mock<IEmailService> CreateEmailService()
        {
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.IsConfigured()).Returns(true);
            return emailServiceMock;
        }
    }

    [TestOf(typeof(SendRemindersNowJob))]
    public class SendRemindersNowJobTests
    {
        internal static IUnitOfWork CreateExecutionUnitOfWork(Mock<ISQLQuery> query = null)
        {
            if (query == null)
            {
                query = new Mock<ISQLQuery>();
                query.Setup(x => x.UniqueResultAsync<bool>(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            }
            query.Setup(x => x.SetString(It.IsAny<string>(), It.IsAny<string>())).Returns(query.Object);
            var session = new Mock<ISession>();
            session.Setup(x => x.CreateSQLQuery(It.IsAny<string>())).Returns(query.Object);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.Session).Returns(session.Object);
            return unitOfWork.Object;
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task when_duplicate_request_executes_then_status_is_not_corrupted(bool afterDelivery)
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(), QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var statuses = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var outcomes = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            var query = new Mock<ISQLQuery>();
            query.SetupSequence(x => x.UniqueResultAsync<bool>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true).ReturnsAsync(false);
            var locator = new Mock<IServiceLocator>();
            locator.Setup(x => x.GetInstance<IUnitOfWork>()).Returns(CreateExecutionUnitOfWork(query));
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>()).Returns(statuses);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>()).Returns(outcomes);
            var sender = new Mock<IReminderEmailSender>();
            locator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var executor = new Mock<IRootScopeExecutor>();
            SendRemindersNowJob job = null;
            var deliveryFinished = false;
            var duplicateExecuted = false;
            executor.Setup(x => x.ExecuteAsync<SendRemindersNowStatus>(
                    It.IsAny<Func<IServiceLocator, Task<SendRemindersNowStatus>>>()))
                .Returns((Func<IServiceLocator, Task<SendRemindersNowStatus>> action) => action(locator.Object));
            executor.Setup(x => x.ExecuteAsync<ReminderEmailSendResult>(
                    It.IsAny<Func<IServiceLocator, Task<ReminderEmailSendResult>>>()))
                .Returns((Func<IServiceLocator, Task<ReminderEmailSendResult>> action) => action(locator.Object));
            executor.Setup(x => x.ExecuteAsync<bool>(It.IsAny<Func<IServiceLocator, Task<bool>>>()))
                .Returns(async (Func<IServiceLocator, Task<bool>> action) =>
                {
                    if (deliveryFinished && !duplicateExecuted)
                    {
                        duplicateExecuted = true;
                        await job.Execute(request, Mock.Of<IJobExecutionContext>());
                    }
                    return await action(locator.Object);
                });
            sender.Setup(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type,
                    0, request.RequestId))
                .Returns(async () =>
                {
                    if (!afterDelivery)
                    {
                        duplicateExecuted = true;
                        await job.Execute(request, Mock.Of<IJobExecutionContext>());
                        Assert.That(statuses.GetById(request.StatusStorageKey).State,
                            Is.EqualTo(SendRemindersNowState.InProgress));
                    }
                    deliveryFinished = true;
                    return new ReminderEmailSendResult { SentCount = 1 };
                });
            job = new SendRemindersNowJob(executor.Object, Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            Assert.That(duplicateExecuted, Is.True);
            Assert.That(statuses.GetById(request.StatusStorageKey).State, Is.EqualTo(SendRemindersNowState.Completed));
            Assert.That(statuses.GetById(request.StatusStorageKey).SentCount, Is.EqualTo(1));
            sender.Verify(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type,
                0, request.RequestId), Times.Once);
            query.Verify(x => x.SetString("lockKey", $"ReminderExecution-{request.RequestId:N}"), Times.Exactly(2));
        }

        [TestCase(0, 0, SendRemindersNowState.Failed, "The reminder emails could not be sent.")]
        [TestCase(0, 2, SendRemindersNowState.Failed, "The reminder emails could not be sent.")]
        [TestCase(1, 0, SendRemindersNowState.PartiallyFailed, "Some reminder emails could not be sent.")]
        [TestCase(1, 2, SendRemindersNowState.PartiallyFailed, "Some reminder emails could not be sent.")]
        public async Task when_exception_follows_committed_delivery_then_counts_and_error_are_retained(
            int sent, int failed, SendRemindersNowState expectedState, string error)
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(), QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var statuses = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var outcomes = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            var locator = new Mock<IServiceLocator>();
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>()).Returns(statuses);
            locator.Setup(x => x.GetInstance<IUnitOfWork>()).Returns(CreateExecutionUnitOfWork());
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>()).Returns(outcomes);
            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type,
                    0, request.RequestId))
                .Callback(() => outcomes.Store(new ReminderEmailDeliveryOutcomes
                    { SentCount = sent, FailedCount = failed }, ReminderEmailDeliveryOutcomes.GetStorageKey(request.RequestId)))
                .ThrowsAsync(new InvalidOperationException("Sensitive provider detail"));
            locator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var job = new SendRemindersNowJob(new NoScopeInScopeExecutor(locator.Object),
                Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            var status = statuses.GetById(request.StatusStorageKey);
            Assert.That(status.SentCount, Is.EqualTo(sent));
            Assert.That(status.FailedCount, Is.EqualTo(failed));
            Assert.That(status.State, Is.EqualTo(expectedState));
            Assert.That(status.Error, Is.EqualTo(error));
        }

        [Test]
        public async Task when_immediate_delivery_is_locked_then_status_reports_specific_failure()
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(), QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(),
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var statuses = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var locator = new Mock<IServiceLocator>();
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>()).Returns(statuses);
            locator.Setup(x => x.GetInstance<IUnitOfWork>()).Returns(CreateExecutionUnitOfWork());
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>())
                .Returns(new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>());
            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type,
                    0, request.RequestId))
                .ThrowsAsync(new ReminderDeliveryInProgressException());
            locator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var job = new SendRemindersNowJob(new NoScopeInScopeExecutor(locator.Object),
                Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            Assert.That(statuses.GetById(request.StatusStorageKey).State, Is.EqualTo(SendRemindersNowState.Failed));
            Assert.That(statuses.GetById(request.StatusStorageKey).Error,
                Is.EqualTo(new ReminderDeliveryInProgressException().Message));
        }

        [TestCase(2, 0, 0, SendRemindersNowState.Completed)]
        [TestCase(0, 2, 0, SendRemindersNowState.Failed)]
        [TestCase(1, 1, 0, SendRemindersNowState.PartiallyFailed)]
        [TestCase(1, 0, 1, SendRemindersNowState.PartiallyFailed)]
        public async Task when_email_delivery_result_is_known_then_status_reflects_it(
            int sentCount, int failedCount, int unconfirmedCount, SendRemindersNowState expectedState)
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 1),
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var statusStorage = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var outcomes = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            var invitationChunks = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryInvitationChunk>();
            var checkpoints = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryCheckpoint>();
            var unresolvedDeliveries = new TestInMemoryKeyValueStorage<ReminderEmailUnresolvedDelivery>();
            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>())
                .Returns(statusStorage);
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>())
                .Returns(outcomes);
            serviceLocator.Setup(x => x.GetInstance<IUnitOfWork>()).Returns(CreateExecutionUnitOfWork());

            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type, 0,
                    request.RequestId))
                .Callback(() =>
                {
                    outcomes.Store(new ReminderEmailDeliveryOutcomes
                    {
                        SentCount = sentCount,
                        FailedCount = failedCount,
                        UnconfirmedCount = unconfirmedCount,
                        InvitationCount = 1,
                        InvitationChunkCount = 1
                    }, ReminderEmailDeliveryOutcomes.GetStorageKey(request.RequestId));
                    invitationChunks.Store(new ReminderEmailDeliveryInvitationChunk
                    {
                        InvitationIds = new List<int> { 1 }
                    }, ReminderEmailDeliveryInvitationChunk.GetStorageKey(request.RequestId, 0));
                    checkpoints.Store(new ReminderEmailDeliveryCheckpoint { Sent = sentCount > 0 },
                        ReminderEmailDeliveryCheckpoint.GetStorageKey(request.RequestId, 1));
                })
                .ReturnsAsync(new ReminderEmailSendResult
                {
                    SentCount = sentCount,
                    FailedCount = failedCount,
                    UnconfirmedCount = unconfirmedCount
                });

            serviceLocator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>())
                .Returns(checkpoints);
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryInvitationChunk>>())
                .Returns(invitationChunks);
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>())
                .Returns(unresolvedDeliveries);
            var job = new SendRemindersNowJob(new NoScopeInScopeExecutor(serviceLocator.Object),
                Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            var status = statusStorage.GetById(request.StatusStorageKey);
            Assert.That(status.State, Is.EqualTo(expectedState));
            Assert.That(status.SentCount, Is.EqualTo(sentCount));
            Assert.That(status.FailedCount, Is.EqualTo(failedCount));
            Assert.That(status.UnconfirmedCount, Is.EqualTo(unconfirmedCount));
            Assert.That(statusStorage.GetById(request.StorageKey).State, Is.EqualTo(expectedState));
            Assert.That(outcomes.GetById(ReminderEmailDeliveryOutcomes.GetStorageKey(request.RequestId)), Is.Null);
            Assert.That(checkpoints.GetById(
                ReminderEmailDeliveryCheckpoint.GetStorageKey(request.RequestId, 1)), Is.Null);
            Assert.That(invitationChunks.GetById(
                ReminderEmailDeliveryInvitationChunk.GetStorageKey(request.RequestId, 0)), Is.Null);
            Assert.That(statusStorage.GetById(request.StatusStorageKey), Is.Not.Null);
        }

        [Test]
        public async Task when_recovering_request_then_persisted_delivery_counts_are_included_in_status()
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 1),
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var statusStorage = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var outcomes = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            outcomes.Store(new ReminderEmailDeliveryOutcomes { SentCount = 1 },
                ReminderEmailDeliveryOutcomes.GetStorageKey(request.RequestId));
            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>())
                .Returns(statusStorage);
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>())
                .Returns(outcomes);
            serviceLocator.Setup(x => x.GetInstance<IUnitOfWork>()).Returns(CreateExecutionUnitOfWork());

            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type, 0,
                    request.RequestId))
                .ReturnsAsync(new ReminderEmailSendResult { FailedCount = 1 });
            serviceLocator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var job = new SendRemindersNowJob(new NoScopeInScopeExecutor(serviceLocator.Object),
                Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            var status = statusStorage.GetById(request.StatusStorageKey);
            Assert.That(status.SentCount, Is.EqualTo(1));
            Assert.That(status.FailedCount, Is.EqualTo(1));
            Assert.That(status.State, Is.EqualTo(SendRemindersNowState.PartiallyFailed));
        }

        [Test]
        public async Task when_job_fails_with_an_in_flight_delivery_then_status_reports_unconfirmed_delivery()
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 1),
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var statuses = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var outcomes = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryOutcomes>();
            var invitationChunks = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryInvitationChunk>();
            outcomes.Store(new ReminderEmailDeliveryOutcomes { InvitationCount = 1, InvitationChunkCount = 1 },
                ReminderEmailDeliveryOutcomes.GetStorageKey(request.RequestId));
            invitationChunks.Store(new ReminderEmailDeliveryInvitationChunk { InvitationIds = new List<int> { 1 } },
                ReminderEmailDeliveryInvitationChunk.GetStorageKey(request.RequestId, 0));
            var checkpoints = new TestInMemoryKeyValueStorage<ReminderEmailDeliveryCheckpoint>();
            checkpoints.Store(new ReminderEmailDeliveryCheckpoint { AttemptInProgress = true },
                ReminderEmailDeliveryCheckpoint.GetStorageKey(request.RequestId, 1));

            var locator = new Mock<IServiceLocator>();
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>()).Returns(statuses);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryOutcomes>>()).Returns(outcomes);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryCheckpoint>>())
                .Returns(checkpoints);
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailDeliveryInvitationChunk>>())
                .Returns(invitationChunks);
            var unresolvedDeliveries = new TestInMemoryKeyValueStorage<ReminderEmailUnresolvedDelivery>();
            locator.Setup(x => x.GetInstance<IPlainKeyValueStorage<ReminderEmailUnresolvedDelivery>>())
                .Returns(unresolvedDeliveries);
            locator.Setup(x => x.GetInstance<IUnitOfWork>()).Returns(CreateExecutionUnitOfWork());
            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type,
                    0, request.RequestId))
                .ThrowsAsync(new InvalidOperationException("The delivery transaction failed."));
            locator.Setup(x => x.GetInstance<IReminderEmailSender>()).Returns(sender.Object);
            var job = new SendRemindersNowJob(new NoScopeInScopeExecutor(locator.Object),
                Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            var status = statuses.GetById(request.StatusStorageKey);
            Assert.That(status.State, Is.EqualTo(SendRemindersNowState.Failed));
            Assert.That(status.UnconfirmedCount, Is.EqualTo(1));
            Assert.That(status.Error, Does.Contain("could not be confirmed"));
            Assert.That(outcomes.GetById(ReminderEmailDeliveryOutcomes.GetStorageKey(request.RequestId)), Is.Null);
            Assert.That(checkpoints.GetById(
                ReminderEmailDeliveryCheckpoint.GetStorageKey(request.RequestId, 1)), Is.Null);
            Assert.That(unresolvedDeliveries.GetById(ReminderEmailUnresolvedDelivery.GetStorageKey(
                request.QuestionnaireIdentity, 1)), Is.Not.Null);
        }
    }

    [TestOf(typeof(SendRemindersNowService))]
    public class SendRemindersNowServiceTests
    {
        [Test]
        public async Task when_creating_request_then_queued_status_is_stored_before_scheduling()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, _) = CreateService();
            var requestId = Guid.NewGuid();
            scheduledTask.Setup(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()))
                .Callback<SendRemindersNowRequest>(request =>
                    Assert.That(storage.GetById(request.StatusStorageKey).State, Is.EqualTo(SendRemindersNowState.Queued)))
                .Returns(Task.CompletedTask);

            var status = await service.SendAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, requestId);

            Assert.That(status.State, Is.EqualTo(SendRemindersNowState.Queued));
            Assert.That(status.RequestId, Is.EqualTo(requestId));
            Assert.That(storage.GetById(SendRemindersNowRequest.GetStorageKey(questionnaireIdentity,
                EmailTextTemplateType.Reminder_NoResponse)).State, Is.EqualTo(SendRemindersNowState.Queued));
            scheduledTask.Verify(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()), Times.Once);
        }

        [Test]
        public async Task when_retrying_with_same_request_id_after_completion_then_existing_request_is_returned()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, _) = CreateService();
            scheduledTask.Setup(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()))
                .Returns(Task.CompletedTask);
            var requestId = Guid.NewGuid();

            var original = await service.SendAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, requestId);
            original.State = SendRemindersNowState.Completed;
            storage.Store(original, SendRemindersNowRequest.GetStatusStorageKey(requestId));
            storage.Store(original, SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, original.Type));

            var retry = await service.SendAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, requestId);

            Assert.That(retry.RequestId, Is.EqualTo(requestId));
            Assert.That(retry.State, Is.EqualTo(SendRemindersNowState.Completed));
            scheduledTask.Verify(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()), Times.Once);
        }

        [Test]
        public async Task when_retrying_a_request_that_reused_an_active_batch_then_the_original_batch_is_returned()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, _) = CreateService();
            scheduledTask.Setup(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()))
                .Returns(Task.CompletedTask);
            var activeStatus = new SendRemindersNowStatus
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = questionnaireIdentity,
                QuestionnaireTitle = "Web Survey",
                Type = EmailTextTemplateType.Reminder_NoResponse,
                State = SendRemindersNowState.Queued
            };
            storage.Store(activeStatus, SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, activeStatus.Type));
            storage.Store(activeStatus, SendRemindersNowRequest.GetStatusStorageKey(activeStatus.RequestId));
            var operationId = Guid.NewGuid();

            var originalResponse = await service.SendAsync(questionnaireIdentity, "Web Survey",
                activeStatus.Type, operationId);
            activeStatus.State = SendRemindersNowState.Completed;
            storage.Store(activeStatus, SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, activeStatus.Type));
            storage.Store(activeStatus, SendRemindersNowRequest.GetStatusStorageKey(activeStatus.RequestId));

            var statusLookup = await service.GetStatusAsync(questionnaireIdentity, operationId);
            var retry = await service.SendAsync(questionnaireIdentity, "Web Survey", activeStatus.Type, operationId);

            Assert.That(originalResponse.RequestId, Is.EqualTo(activeStatus.RequestId));
            Assert.That(statusLookup.RequestId, Is.EqualTo(activeStatus.RequestId));
            Assert.That(retry.RequestId, Is.EqualTo(activeStatus.RequestId));
            Assert.That(retry.State, Is.EqualTo(SendRemindersNowState.Completed));
            scheduledTask.Verify(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()), Times.Once);
        }

        [Test]
        public async Task when_active_request_has_no_quartz_trigger_then_status_poll_reschedules_same_request()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, _) = CreateService();
            var status = new SendRemindersNowStatus
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = questionnaireIdentity,
                QuestionnaireTitle = "Web Survey",
                Type = EmailTextTemplateType.Reminder_NoResponse,
                State = SendRemindersNowState.InProgress
            };
            storage.Store(status, SendRemindersNowRequest.GetStatusStorageKey(status.RequestId));
            scheduledTask.Setup(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()))
                .Callback<SendRemindersNowRequest>(request =>
                    Assert.That(request.RequestId, Is.EqualTo(status.RequestId)))
                .Returns(Task.CompletedTask);

            var returnedStatus = await service.GetStatusAsync(questionnaireIdentity, status.RequestId);

            Assert.That(returnedStatus.RequestId, Is.EqualTo(status.RequestId));
            scheduledTask.Verify(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()), Times.Once);
        }

        [Test]
        public async Task when_request_is_already_queued_then_existing_request_is_reused()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, scheduler) = CreateService();
            var status = new SendRemindersNowStatus
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = questionnaireIdentity,
                QuestionnaireTitle = "Web Survey",
                Type = EmailTextTemplateType.Reminder_NoResponse,
                State = SendRemindersNowState.Queued
            };
            storage.Store(status, SendRemindersNowRequest.GetStorageKey(questionnaireIdentity, status.Type));
            var request = new SendRemindersNowRequest
            {
                RequestId = status.RequestId,
                QuestionnaireIdentity = questionnaireIdentity,
                QuestionnaireTitle = status.QuestionnaireTitle,
                Type = status.Type
            };
            var trigger = new Mock<ITrigger>();
            trigger.SetupGet(x => x.JobDataMap).Returns(new JobDataMap
            {
                [BaseTask.TaskDataKey] = JsonSerializer.Serialize(request)
            });
            scheduler.Setup(x => x.GetTriggersOfJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { trigger.Object });

            var returnedStatus = await service.SendAsync(questionnaireIdentity, "Web Survey", status.Type,
                Guid.NewGuid());

            Assert.That(returnedStatus.RequestId, Is.EqualTo(status.RequestId));
            scheduledTask.Verify(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()), Times.Never);
        }

        [Test]
        public async Task when_scheduling_throws_after_request_was_published_then_request_is_not_failed()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, scheduler) = CreateService();
            ITrigger publishedTrigger = null;
            scheduler.Setup(x => x.GetTriggersOfJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => publishedTrigger == null ? Array.Empty<ITrigger>() : new[] { publishedTrigger });
            scheduledTask.Setup(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()))
                .Callback<SendRemindersNowRequest>(request =>
                {
                    publishedTrigger = new Mock<ITrigger>().Object;
                    Mock.Get(publishedTrigger).SetupGet(x => x.JobDataMap).Returns(new JobDataMap
                    {
                        [BaseTask.TaskDataKey] = JsonSerializer.Serialize(request)
                    });
                })
                .ThrowsAsync(new InvalidOperationException("Request was already published"));

            var status = await service.SendAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, Guid.NewGuid());

            Assert.That(status.State, Is.EqualTo(SendRemindersNowState.Queued));
            Assert.That(storage.GetById(SendRemindersNowRequest.GetStatusStorageKey(status.RequestId)).State,
                Is.EqualTo(SendRemindersNowState.Queued));
        }

        [Test]
        public async Task when_scheduling_throws_after_request_started_then_request_is_not_failed()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, _) = CreateService();
            scheduledTask.Setup(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()))
                .Callback<SendRemindersNowRequest>(request =>
                {
                    var status = storage.GetById(request.StatusStorageKey);
                    status.State = SendRemindersNowState.InProgress;
                    status.StartedAtUtc = DateTime.UtcNow;
                })
                .ThrowsAsync(new InvalidOperationException("Request has started"));

            var status = await service.SendAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, Guid.NewGuid());

            Assert.That(status.State, Is.Not.EqualTo(SendRemindersNowState.Failed));
            var persistedStatus = storage.GetById(SendRemindersNowRequest.GetStatusStorageKey(status.RequestId));
            Assert.That(persistedStatus.State, Is.EqualTo(SendRemindersNowState.InProgress));
            Assert.That(persistedStatus.StartedAtUtc, Is.Not.Null);
        }

        [Test]
        public async Task when_scheduling_fails_without_publishing_request_then_request_is_failed()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);
            var (service, storage, scheduledTask, _) = CreateService();
            scheduledTask.Setup(x => x.Schedule(It.IsAny<SendRemindersNowRequest>()))
                .ThrowsAsync(new InvalidOperationException("Scheduling failed"));

            var status = await service.SendAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, Guid.NewGuid());

            Assert.That(status.State, Is.EqualTo(SendRemindersNowState.Failed));
            Assert.That(storage.GetById(SendRemindersNowRequest.GetStatusStorageKey(status.RequestId)).State,
                Is.EqualTo(SendRemindersNowState.Failed));
        }

        private static (SendRemindersNowService, TestInMemoryKeyValueStorage<SendRemindersNowStatus>,
            Mock<IScheduledTask<SendRemindersNowJob, SendRemindersNowRequest>>, Mock<IScheduler>) CreateService()
        {
            var storage = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var aliases = new TestInMemoryKeyValueStorage<SendRemindersNowRequestIdAlias>();
            var sqlQuery = new Mock<ISQLQuery>();
            sqlQuery.Setup(x => x.SetString(It.IsAny<string>(), It.IsAny<string>())).Returns(sqlQuery.Object);
            sqlQuery.Setup(x => x.UniqueResultAsync<bool>(It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var session = new Mock<ISession>();
            session.Setup(x => x.CreateSQLQuery(It.IsAny<string>())).Returns(sqlQuery.Object);
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(x => x.Session).Returns(session.Object);

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>())
                .Returns(storage);
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowRequestIdAlias>>())
                .Returns(aliases);
            serviceLocator.Setup(x => x.GetInstance<IUnitOfWork>()).Returns(unitOfWork.Object);

            var rootScopeExecutor = new Mock<IRootScopeExecutor>();
            rootScopeExecutor.Setup(x => x.ExecuteAsync<SendRemindersNowStatus>(
                    It.IsAny<Func<IServiceLocator, Task<SendRemindersNowStatus>>>()))
                .Returns((Func<IServiceLocator, Task<SendRemindersNowStatus>> action) =>
                    action(serviceLocator.Object));
            rootScopeExecutor.Setup(x => x.ExecuteAsync<bool>(It.IsAny<Func<IServiceLocator, Task<bool>>>()))
                .Returns((Func<IServiceLocator, Task<bool>> action) => action(serviceLocator.Object));

            var scheduler = new Mock<IScheduler>();
            scheduler.Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<IJobExecutionContext>());
            scheduler.Setup(x => x.GetTriggersOfJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ITrigger>());
            var schedulerFactory = new Mock<ISchedulerFactory>();
            schedulerFactory.Setup(x => x.GetScheduler(It.IsAny<CancellationToken>()))
                .ReturnsAsync(scheduler.Object);

            var scheduledTask = new Mock<IScheduledTask<SendRemindersNowJob, SendRemindersNowRequest>>();
            var service = new SendRemindersNowService(rootScopeExecutor.Object, scheduledTask.Object,
                schedulerFactory.Object);
            return (service, storage, scheduledTask, scheduler);
        }
    }
}

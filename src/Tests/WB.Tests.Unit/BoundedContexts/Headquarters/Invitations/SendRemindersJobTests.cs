using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(ReminderEmailSender))]
    public class ReminderEmailSenderTests
    {
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
        public async Task when_email_delivery_fails_then_result_reports_failure()
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
                .ThrowsAsync(new EmailServiceException("respondent@email.com", HttpStatusCode.BadRequest));

            var configProvider = new Mock<IWebInterviewConfigProvider>();
            configProvider.Setup(x => x.Get(questionnaireIdentity)).Returns(new WebInterviewConfig { Started = true });
            var emailRenderer = new Mock<IWebInterviewEmailRenderer>();
            emailRenderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
                .ReturnsAsync(new PersonalizedWebInterviewEmail("subject", "html", "text"));

            var sender = Create.Service.ReminderEmailSender(
                invitationService: invitationService.Object,
                emailService: emailService.Object,
                webInterviewConfigProvider: configProvider.Object,
                webInterviewEmailRenderer: emailRenderer.Object);

            var result = await sender.SendRemindersAsync(questionnaireIdentity, "Web Survey",
                EmailTextTemplateType.Reminder_NoResponse, 0);

            Assert.That(result.SentCount, Is.Zero);
            Assert.That(result.FailedCount, Is.EqualTo(1));
            invitationService.Verify(x => x.ReminderWasNotSent(invitation.Id, invitation.AssignmentId,
                "respondent@email.com", It.IsAny<string>()), Times.Once);
        }
    }

    [TestOf(typeof(SendRemindersJob))]
    public class SendRemindersJobTests
    {
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
        [TestCase(2, 0, SendRemindersNowState.Completed)]
        [TestCase(0, 2, SendRemindersNowState.Failed)]
        [TestCase(1, 1, SendRemindersNowState.PartiallyFailed)]
        public async Task when_email_delivery_result_is_known_then_status_reflects_it(
            int sentCount, int failedCount, SendRemindersNowState expectedState)
        {
            var request = new SendRemindersNowRequest
            {
                RequestId = Guid.NewGuid(),
                QuestionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 1),
                Type = EmailTextTemplateType.Reminder_NoResponse
            };
            var statusStorage = new TestInMemoryKeyValueStorage<SendRemindersNowStatus>();
            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IPlainKeyValueStorage<SendRemindersNowStatus>>())
                .Returns(statusStorage);

            var sender = new Mock<IReminderEmailSender>();
            sender.Setup(x => x.SendRemindersAsync(request.QuestionnaireIdentity, It.IsAny<string>(), request.Type, 0))
                .ReturnsAsync(new ReminderEmailSendResult { SentCount = sentCount, FailedCount = failedCount });

            var job = new SendRemindersNowJob(
                sender.Object,
                new NoScopeInScopeExecutor(serviceLocator.Object),
                Mock.Of<ILogger<SendRemindersNowJob>>());

            await job.Execute(request, Mock.Of<IJobExecutionContext>());

            var status = statusStorage.GetById(request.StorageKey);
            Assert.That(status.State, Is.EqualTo(expectedState));
            Assert.That(status.SentCount, Is.EqualTo(sentCount));
            Assert.That(status.FailedCount, Is.EqualTo(failedCount));
        }
    }
}

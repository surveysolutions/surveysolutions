﻿using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(SendRemindersJob))]
    public class SendRemindersJobTests
    {
        [Test]
        public void when_email_service_is_not_configured()
        {
            var invitationServiceMock = new Mock<IInvitationService>();
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.IsConfigured()).Returns(false);

            //arrange 
            var job = Create.Service.SendRemindersJob(invitationService: invitationServiceMock.Object, emailService: emailServiceMock.Object);

            //act
            job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            invitationServiceMock.Verify(x => x.GetQuestionnairesWithInvitations(), Times.Never);
        }

        [Test]
        public void when_reminders_are_turned_off()
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
            job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            invitationServiceMock.Verify(x => x.GetNoResponseInvitations(Create.Entity.QuestionnaireIdentity(Id.g1, 10), It.IsAny<int>()), Times.Never);
            invitationServiceMock.Verify(x => x.GetPartialResponseInvitations(Create.Entity.QuestionnaireIdentity(Id.g2, 2), It.IsAny<int>()), Times.Never);
        }

        // TODO: Fix tests HQ Core migration
        //[Test]
        //public void when_sending_partial_response_reminders()
        //{
        //    //arrange 
        //    var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g1, 10);

        //    List<SentEmailForTests> sentEmails = new List<SentEmailForTests>();
        //    var emailParameters = new TestInMemoryKeyValueStorage<EmailParameters>();

        //    var emailService = CreateEmailService()
        //        .WithSenderInfo()
        //        .CollectSentEmails(sentEmails, new [] {"email_id_1", "email_id_2"});

        //    var invitationServiceMock = CreateInvitationService()
        //        .WithQuestionnaires(Create.Entity.QuestionnaireBrowseItem(Id.g1, 10, title: "Web Survey"))
        //        .WithPartialInvitations(questionnaireIdentity, 2, 4)
        //        .WithInvitations(
        //            Create.Entity.Invitation(2, Create.Entity.Assignment(email: "one@email.com", password: "AAAAAAAA"), token: "token1"), 
        //            Create.Entity.Invitation(4, Create.Entity.Assignment(email: "two@email.com", password: "BBBBBBBB"), token: "token2"));
            
        //    var settingsMock = new Mock<IWebInterviewConfigProvider>();
        //    var settings = Mock.Of<WebInterviewConfig>(_ => 
        //        _.BaseUrl == "http://localhost" &&
        //        _.ReminderAfterDaysIfPartialResponse == 2 &&
        //        _.GetEmailTemplate(EmailTextTemplateType.Reminder_PartialResponse) == Create.Entity.EmailTemplate("Subject: %SURVEYNAME%", "%SURVEYNAME% %PASSWORD% %SURVEYLINK%", "", ""));

        //    settingsMock.Setup(x => x.Get(questionnaireIdentity)).Returns(settings);

        //    var personalizedWebInterviewEmail = new PersonalizedWebInterviewEmail("subject", "html", "text");
        //    var paramsIds = new List<string>();
        //    var webInterviewEmailRenderer = new Mock<IWebInterviewEmailRenderer>();
        //    webInterviewEmailRenderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
        //        .Returns(personalizedWebInterviewEmail)
        //        .Callback<EmailParams>(x => paramsIds.Add(x.Id));

        //    var job = Create.Service.SendRemindersJob(
        //        invitationService: invitationServiceMock.Object, 
        //        webInterviewConfigProvider: settingsMock.Object,
        //        emailService: emailService.Object,
        //        emailParamsStorage: emailParameters,
        //        webInterviewEmailRenderer: webInterviewEmailRenderer.Object);

        //    //act
        //    job.Execute(Mock.Of<IJobExecutionContext>());

        //    //assert
        //    Assert.That(sentEmails.Count, Is.EqualTo(2));
            
        //    Assert.That(sentEmails[0].Email, Is.EqualTo("one@email.com"));
        //    var paramsId = paramsIds[0];
        //    Assert.That(emailParameters.GetById(paramsId).Password, Is.EqualTo("AAAAAAAA"));
        //    Assert.That(emailParameters.GetById(paramsId).SurveyName, Is.EqualTo("Web Survey"));
        //    Assert.That(emailParameters.GetById(paramsId).Link, Is.EqualTo("http://localhost/WebInterview/token1/Start"));

        //    Assert.That(sentEmails[1].Email, Is.EqualTo("two@email.com"));
        //    paramsId = paramsIds[1];
        //    Assert.That(emailParameters.GetById(paramsId).Password, Is.EqualTo("BBBBBBBB"));
        //    Assert.That(emailParameters.GetById(paramsId).SurveyName, Is.EqualTo("Web Survey"));
        //    Assert.That(emailParameters.GetById(paramsId).Link, Is.EqualTo("http://localhost/WebInterview/token2/Start"));
           
        //    invitationServiceMock.Verify(x => x.MarkInvitationAsReminded(2, "email_id_1"), Times.Once);
        //    invitationServiceMock.Verify(x => x.MarkInvitationAsReminded(4, "email_id_2"), Times.Once);
        //}

        //[Test]
        //public void when_sending_no_response_reminders()
        //{
        //    //arrange 
        //    var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(Id.g2, 2);

        //    List<SentEmailForTests> sentEmails = new List<SentEmailForTests>();

        //    var emailService = CreateEmailService()
        //        .WithSenderInfo()
        //        .CollectSentEmails(sentEmails, new [] {"email_id_1"});

        //    var invitationServiceMock = CreateInvitationService()
        //        .WithQuestionnaires(Create.Entity.QuestionnaireBrowseItem(Id.g2, 2, title: "Web Survey"))
        //        .WithNoResponseInvitations(questionnaireIdentity, 1)
        //        .WithInvitations(Create.Entity.Invitation(1, Create.Entity.Assignment(email: "some@email.com", password: "AAAAAAAA"), token: "token"));
            
        //    var emailParameters = new TestInMemoryKeyValueStorage<EmailParameters>();

        //    var settingsMock = new Mock<IWebInterviewConfigProvider>();
        //    var settings = Mock.Of<WebInterviewConfig>(_ => 
        //        _.BaseUrl == "http://localhost" &&
        //        _.ReminderAfterDaysIfNoResponse == 2 && 
        //        _.GetEmailTemplate(EmailTextTemplateType.Reminder_NoResponse) == Create.Entity.EmailTemplate("Subject: %SURVEYNAME%", "%SURVEYNAME% %PASSWORD% %SURVEYLINK%", "", ""));

        //    settingsMock.Setup(x => x.Get(questionnaireIdentity)).Returns(settings);

        //    var personalizedWebInterviewEmail = new PersonalizedWebInterviewEmail("subject", "html", "text");
        //    var webInterviewEmailRenderer = new Mock<IWebInterviewEmailRenderer>();
        //    webInterviewEmailRenderer.Setup(x => x.RenderEmail(It.IsAny<EmailParams>()))
        //        .Returns(personalizedWebInterviewEmail);

        //    var job = Create.Service.SendRemindersJob(
        //        invitationService: invitationServiceMock.Object, 
        //        webInterviewConfigProvider: settingsMock.Object,
        //        emailService: emailService.Object,
        //        emailParamsStorage: emailParameters,
        //        webInterviewEmailRenderer: webInterviewEmailRenderer.Object);

        //    //act
        //    job.Execute(Mock.Of<IJobExecutionContext>());

        //    //assert
        //    Assert.That(sentEmails.Count, Is.EqualTo(1));
        //    Assert.That(sentEmails[0].Email, Is.EqualTo("some@email.com"));
        //    Assert.That(emailParameters.ValueAt(0).Password, Is.EqualTo("AAAAAAAA"));
        //    Assert.That(emailParameters.ValueAt(0).SurveyName, Is.EqualTo("Web Survey"));
        //    Assert.That(emailParameters.ValueAt(0).Link, Is.EqualTo("http://localhost/WebInterview/token/Start"));
           
        //    invitationServiceMock.Verify(x => x.MarkInvitationAsReminded(1, "email_id_1"), Times.Once);
        //}

        private Mock<IInvitationService> CreateInvitationService()
        {
            return new Mock<IInvitationService>();
        }

        private Mock<IEmailService> CreateEmailService()
        {
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.IsConfigured()).Returns(true);
            return emailServiceMock;
        }
    }
}

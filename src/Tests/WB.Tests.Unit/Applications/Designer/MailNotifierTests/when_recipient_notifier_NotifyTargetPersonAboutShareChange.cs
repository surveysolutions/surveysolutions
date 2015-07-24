﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Mvc.Mailer;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.Models;
using it = Moq.It;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.MailNotifierTests
{
    internal class when_recipient_notifier_NotifyTargetPersonAboutShareChange
    {
        Establish context = () =>
        {
            systemMailer.Setup(x => x.GetShareChangeNotificationEmail(it.IsAny<SharingNotificationModel>())).Returns(message.Object);

            recipientNotifier = new MailNotifier(systemMailer.Object);
        };

        Because of = () =>
            recipientNotifier.NotifyTargetPersonAboutShareChange(ShareChangeType.Share, receiverEmail, receiverName, questoinnaireId, questionnaiteTitle, ShareType.View, actionUserEmail);

        It should_call_SendAsync = () =>
           message.Verify(x => x.SendAsync(null, null), Times.Once);

        It should_call_GetShareChangeNotificationEmail = () =>
           systemMailer.Verify(x => x.GetShareChangeNotificationEmail(Moq.It.Is<SharingNotificationModel>(
               y => y.Email == receiverEmail.ToWBEmailAddress()
               && y.ShareTypeName == "view" 
               && y.QuestionnaireId == questoinnaireId
               && y.UserCallName == receiverName
               && y.QuestionnaireDisplayTitle == questionnaiteTitle
               && y.ActionPersonCallName == actionUserEmail
               )), Times.Once);

        private static Guid responsibleId = Guid.Parse("23333333333333333333333333333333");
        private static Guid questoinnaireId = Guid.Parse("13333333333333333333333333333333");
        
        private static string receiverEmail = "test@example.com";
        private static string receiverName = "receiverName";
        
        private static string questionnaiteTitle = "questionnaire title";

        private static Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static string actionUserEmail = "test1@example.com";

        private static Guid ownerId = Guid.Parse("53333333333333333333333333333333");
        //private static string ownerName = "ownerName";
        
        private static Mock<ISystemMailer> systemMailer = new Mock<ISystemMailer>();

        private static Mock<MvcMailMessage> message = new Mock<MvcMailMessage>();

        private static MailNotifier recipientNotifier ;
    }
}
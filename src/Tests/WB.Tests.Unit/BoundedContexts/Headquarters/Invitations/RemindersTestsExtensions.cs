using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    public class SentEmailForTests
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Html { get; set; }
        public string Text { get; set; }
    }

    public static class RemindersTestsExtensions
    {
        
        public static Mock<IEmailService> WithSenderInfo(this Mock<IEmailService> emailService)
        {
            emailService
                .Setup(x => x.GetSenderInfo())
                .Returns(() =>
                {
                    return Mock.Of<ISenderInformation>();
                });
            
            return emailService;
        }

        public static Mock<IEmailService> CollectSentEmails(this Mock<IEmailService> emailService, List<SentEmailForTests> sentEmails, string[] emailIds)
        {
            var queue = new Queue<string>(emailIds);

            emailService
                .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<EmailAttachment>>()))
                .Callback<string,string,string,string, List<EmailAttachment>>((email, subject, html, text, attachments) => 
                {
                    sentEmails.Add(new SentEmailForTests
                    {
                        Email = email,
                        Subject = subject,
                        Html = html,
                        Text = text
                    });
                })
                .Returns(() =>
                {
                    return Task.FromResult(queue.Dequeue());
                });
            
            return emailService;
        }

        public static Mock<IInvitationService> WithInvitations(this Mock<IInvitationService> invitationsService, params Invitation[] invitations)
        {
            foreach (var invitation in invitations)
            {
                invitationsService.Setup(x => x.GetInvitation(invitation.Id)).Returns(invitation);
            }
            
            return invitationsService;
        }

        public static Mock<IInvitationService> WithNoResponseInvitations(this Mock<IInvitationService> invitations, QuestionnaireIdentity questionnaireIdentity, params int[] invitationIds)
        {
            invitations.Setup(x => x.GetNoResponseInvitations(questionnaireIdentity, 2)).Returns(invitationIds);
            return invitations;
        }
        public static Mock<IInvitationService> WithPartialInvitations(this Mock<IInvitationService> invitations, QuestionnaireIdentity questionnaireIdentity, params int[] invitationIds)
        {
            invitations.Setup(x => x.GetPartialResponseInvitations(questionnaireIdentity, 2)).Returns(invitationIds);
            return invitations;
        }

        public static Mock<IInvitationService> WithQuestionnaires(this Mock<IInvitationService> invitations, params QuestionnaireBrowseItem[] questionnaires)
        {
            invitations.Setup(x => x.GetQuestionnairesWithInvitations()).Returns(questionnaires);
            return invitations;
        }

    }
}

using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.Applications.CommandPostProcessorTests
{
    internal class when_command_prostprocessor_handles_AddSharedPersonToQuestionnaireCommand : CommandPostProcessorTestsContext
    {
        Establish context = () =>
        {
            var membershipUserService = Mock.Of<IMembershipUserService>(
                _ => _.WebUser == Mock.Of<IMembershipWebUser>(
                    u => u.UserId == actionUserId && u.MembershipUser.Email == actionUserEmail));

            var questionnaire = CreateQuestionnaireDocument(questionnaiteTitle, ownerId);
            
            var documentStorage = Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<string>()) == questionnaire);

            var accountRepository = new Mock<IAccountRepository>();
            accountRepository.Setup(x => x.GetByProviderKey(ownerId)).Returns(Mock.Of<IMembershipAccount>(
                u => u.ProviderUserKey == ownerId && u.Email == ownerEmail));

            accountRepository.Setup(x => x.GetUserNameByEmail(receiverEmail)).Returns(receiverName);

            command = new AddSharedPersonToQuestionnaire(questoinnaireId, actionUserId, receiverEmail, ShareType.Edit, responsibleId);

            var logger = new Mock<ILogger>();

            commandPostprocessor = Create.CommandPostprocessor(membershipUserService, recipientNotifier.Object, accountRepository.Object, documentStorage, logger.Object);
        };

        Because of = () =>
            commandPostprocessor.ProcessCommandAfterExecution(command);

        It should_call_NotifyTargetPersonAboutShareChange = () =>
           recipientNotifier.Verify(
               x => x.NotifyTargetPersonAboutShareChange(ShareChangeType.Share, receiverEmail, receiverName, questoinnaireId, questionnaiteTitle, ShareType.Edit, actionUserEmail),
               Times.Once);

        It should_call_NotifyOwnerAboutShareChange = () =>
           recipientNotifier.Verify(
               x => x.NotifyOwnerAboutShareChange(ShareChangeType.Share, ownerEmail, null, questoinnaireId, questionnaiteTitle, ShareType.Edit, actionUserEmail, receiverEmail),
               Times.Once);

        private static CommandPostprocessor commandPostprocessor;        
        
        private static Guid responsibleId = Guid.Parse("23333333333333333333333333333333");
        private static Guid questoinnaireId = Guid.Parse("13333333333333333333333333333333");
        
        private static string receiverEmail = "test@example.com";
        private static string receiverName = "receiverName";
        
        private static string questionnaiteTitle = "questionnaire title";

        private static Guid actionUserId = Guid.Parse("33333333333333333333333333333333");
        private static string actionUserEmail = "test1@example.com";

        private static Guid ownerId = Guid.Parse("53333333333333333333333333333333");
        private static string ownerEmail = "test2@example.com";
        //private static string ownerName = "ownerName";
        
        private static Mock<IRecipientNotifier> recipientNotifier = new Mock<IRecipientNotifier>();
        private static ICommand command;
    }
}
using System;
using System.Web.Security;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandPostprocessor : ICommandPostprocessor
    {
        private readonly IMembershipUserService userHelper;
        private readonly IRecipientNotifier notifier;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly IAccountRepository accountRepository;
        private readonly ILogger logger;


        public CommandPostprocessor(IMembershipUserService userHelper, IRecipientNotifier notifier, IAccountRepository accountRepository, IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader, ILogger logger)
        {
            this.userHelper = userHelper;
            this.notifier = notifier;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.accountRepository = accountRepository;
            this.logger = logger;
        }


        public void ProcessCommandAfterExecution(ICommand command)
        {
            try
            {
                var addSharedPersonCommand = command as AddSharedPersonToQuestionnaire;
                if (addSharedPersonCommand != null)
                {
                    this.HandleNotifications(ShareChangeType.Share, addSharedPersonCommand.Email, addSharedPersonCommand.QuestionnaireId, addSharedPersonCommand.ShareType);
                    return;
                }

                var removeSharedPersonCommand = command as RemoveSharedPersonFromQuestionnaire;
                if (removeSharedPersonCommand != null)
                {
                    this.HandleNotifications(ShareChangeType.StopShare, removeSharedPersonCommand.Email, removeSharedPersonCommand.QuestionnaireId, ShareType.Edit);
                }
            }
            catch (Exception exc)
            {
                logger.Error("Error on command post-processing", exc);
            }
        }

        private void HandleNotifications(ShareChangeType shareChangeType, string email, Guid questionnaireId, ShareType shareType)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);

            string actionPersonEmail = this.userHelper.WebUser.MembershipUser.Email;
            string questionnaireTitle = questionnaire.Title;
            string personName = accountRepository.GetUserNameByEmail(email);

            this.notifier.NotifyTargetPersonAboutShareChange(shareChangeType, 
                email,
                personName,
                questionnaireId,
                questionnaireTitle,
                shareType,
                actionPersonEmail);

            if (questionnaire.CreatedBy.HasValue && questionnaire.CreatedBy.Value != this.userHelper.WebUser.UserId)
            {
                var user = accountRepository.GetByProviderKey(questionnaire.CreatedBy.Value);
                if (user != null)
                {
                    this.notifier.NotifyOwnerAboutShareChange(shareChangeType,
                        user.Email,
                        user.UserName,
                        questionnaireId,
                        questionnaireTitle,
                        shareType,
                        actionPersonEmail,
                        email);
                }
            }
        }
    }
}

using System.Web.Security;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandPostprocessor : ICommandPostprocessor
    {
        private readonly IMembershipUserService userHelper;
        private readonly IRecipientNotifier notifier;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;


        public CommandPostprocessor(IMembershipUserService userHelper, IRecipientNotifier notifier, IReadSideKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader)
        {
            this.userHelper = userHelper;
            this.notifier = notifier;
            this.questionnaireDocumentReader = questionnaireDocumentReader;
        }


        public void ProcessCommandAfterExecution(ICommand command)
        {
            var addSharedPersonCommand = command as AddSharedPersonToQuestionnaireCommand;
            if (addSharedPersonCommand != null)
            {
                var questionnaire = this.questionnaireDocumentReader.GetById(addSharedPersonCommand.QuestionnaireId);

                string actionPersonEmail = this.userHelper.WebUser.MembershipUser.Email;
                string questionnaireTitle = questionnaire.Title;
                string personName = Membership.GetUserNameByEmail(addSharedPersonCommand.Email);

                notifier.NotifyTargetPersonAboutShare(addSharedPersonCommand.Email,
                    personName,
                    addSharedPersonCommand.QuestionnaireId,
                    questionnaireTitle,
                    addSharedPersonCommand.ShareType,
                    actionPersonEmail);

                if (questionnaire.CreatedBy.HasValue && questionnaire.CreatedBy.Value != this.userHelper.WebUser.UserId)
                {
                    var user = Membership.GetUser(questionnaire.CreatedBy.Value, false);
                    if (user != null)
                    {
                        notifier.NotifyOwnerAboutShare(
                            user.Email,
                            user.UserName,
                            addSharedPersonCommand.QuestionnaireId,
                            questionnaireTitle,
                            addSharedPersonCommand.ShareType,
                            actionPersonEmail,
                            addSharedPersonCommand.Email);
                    }
                }
            }

            var removeSharedPersonCommand = command as RemoveSharedPersonFromQuestionnaireCommand;
            if (removeSharedPersonCommand != null)
            {
                var questionnaire = this.questionnaireDocumentReader.GetById(removeSharedPersonCommand.QuestionnaireId);

                string actionPersonEmail = this.userHelper.WebUser.MembershipUser.Email;
                string questionnaireTitle = questionnaire.Title;
                string personName = Membership.GetUserNameByEmail(removeSharedPersonCommand.Email);
                
                notifier.NotifyTargetPersonAboutStopShare(removeSharedPersonCommand.Email,
                    personName,
                    questionnaireTitle,
                    actionPersonEmail);
                
                if (questionnaire.CreatedBy.HasValue && questionnaire.CreatedBy.Value != this.userHelper.WebUser.UserId)
                {
                    var user = Membership.GetUser(questionnaire.CreatedBy.Value, false);

                    if (user != null)
                    {
                        notifier.NotifyOwnerAboutStopShare(
                            user.Email,
                            user.UserName,
                            questionnaireTitle,
                            actionPersonEmail,
                            removeSharedPersonCommand.Email);
                    }
                }
            }
        }

    }
}

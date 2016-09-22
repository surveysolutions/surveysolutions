using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IRecipientNotifier
    {
        void NotifyTargetPersonAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail);
        
        void NotifyOwnerAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail, string sharedWithPersonEmail);
        
    }
}

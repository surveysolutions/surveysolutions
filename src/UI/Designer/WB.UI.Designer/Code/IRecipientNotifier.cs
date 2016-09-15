using System;
using Main.Core.Entities.SubEntities;
using WB.UI.Designer.Code.Implementation;

namespace WB.UI.Designer.Code
{
    public interface IRecipientNotifier
    {
        void NotifyTargetPersonAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail);
        
        void NotifyOwnerAboutShareChange(ShareChangeType shareChangeType, string email, string userName, string questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail, string sharedWithPersonEmail);
        
    }
}

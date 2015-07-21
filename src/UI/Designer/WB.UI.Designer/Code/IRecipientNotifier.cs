using System;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Code
{
    public interface IRecipientNotifier
    {
        void NotifyTargetPersonAboutShare(string email, string userName, Guid questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail);
        void NotifyTargetPersonAboutStopShare(string email, string userName, string questionnaireTitle, string actionPersonEmail);

        void NotifyOwnerAboutShare(string email, string userName, Guid questionnaireId, string questionnaireTitle, ShareType shareType, string actionPersonEmail, string sharedWithPersonEmail);
        void NotifyOwnerAboutStopShare(string email, string userName, string questionnaireTitle, string actionPersonEmail, string sharedWithPersonEmail);
    }
}

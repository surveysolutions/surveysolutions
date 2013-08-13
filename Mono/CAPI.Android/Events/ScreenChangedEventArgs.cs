using System;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace CAPI.Android.Events
{
    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangedEventArgs(ItemPublicKey? screenId)
        {
            ScreenId = screenId;
        }
        public ItemPublicKey? ScreenId { get; private set; }
    }
}
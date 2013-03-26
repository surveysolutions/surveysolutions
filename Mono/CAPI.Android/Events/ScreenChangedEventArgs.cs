using System;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;

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
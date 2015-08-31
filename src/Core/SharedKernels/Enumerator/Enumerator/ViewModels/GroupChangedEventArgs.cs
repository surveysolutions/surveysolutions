using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class GroupChangedEventArgs : EventArgs
    {
        public Identity TargetGroup { get; set; }

        public Identity AnchoredElementIdentity { get; set; }

        public ScreenType ScreenType { get; set; }
    }
}
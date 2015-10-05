using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class ScreenChangedEventArgs : EventArgs
    {
        public Identity TargetGroup { get; set; }

        public Identity AnchoredElementIdentity { get; set; }

        public ScreenType TargetScreen { get; set; }
    }
}
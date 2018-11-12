using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangedEventArgs(ScreenType targetStage,
            Identity targetGroup, 
            Identity anchoredElementIdentity, 
            ScreenType previousStage, 
            Identity previousGroup)
        {
            this.TargetStage = targetStage;
            this.TargetGroup = targetGroup;
            this.AnchoredElementIdentity = anchoredElementIdentity;
            this.PreviousStage = previousStage;
            this.PreviousGroup = previousGroup;
        }

        public ScreenType TargetStage { get; }
        public Identity TargetGroup { get; }
        public Identity AnchoredElementIdentity { get; }
        public ScreenType PreviousStage { get; }
        public Identity PreviousGroup { get; }
    }
}

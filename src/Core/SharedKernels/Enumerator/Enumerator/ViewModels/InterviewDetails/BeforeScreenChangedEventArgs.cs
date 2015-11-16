using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class BeforeScreenChangedEventArgs : EventArgs
    {
        public BeforeScreenChangedEventArgs(Identity currentGroup, Identity targetGroup)
        {
            this.CurrentGroup = currentGroup;
            this.TargetGroup = targetGroup;
        }

        public Identity CurrentGroup { get; private set; }

        public Identity TargetGroup { get; private set; }
    }
}
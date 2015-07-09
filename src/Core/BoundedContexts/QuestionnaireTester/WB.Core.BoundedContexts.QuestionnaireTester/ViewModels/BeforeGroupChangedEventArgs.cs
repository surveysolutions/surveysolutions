using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class BeforeGroupChangedEventArgs : EventArgs
    {
        public BeforeGroupChangedEventArgs(Identity currentGroup, Identity targetGroup)
        {
            this.CurrentGroup = currentGroup;
            this.TargetGroup = targetGroup;
        }

        public Identity CurrentGroup { get; private set; }

        public Identity TargetGroup { get; private set; }
    }
}
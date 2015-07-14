using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class GroupChangedEventArgs : EventArgs
    {
        public Identity TargetGroup { get; set; }

        public Identity AnchoredElementIdentity { get; set; }
    }
}
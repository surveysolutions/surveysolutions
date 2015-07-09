using System;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class NavigationParams : EventArgs
    {
        public Identity TargetGroup { get; set; }

        public Identity AnchoredElementIdentity { get; set; }

        protected bool Equals(GroupChangedEventArgs other)
        {
            return Equals(this.TargetGroup, other.TargetGroup) && Equals(this.AnchoredElementIdentity, other.AnchoredElementIdentity);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.TargetGroup != null ? this.TargetGroup.GetHashCode() : 0) * 619) ^ (this.AnchoredElementIdentity != null ? this.AnchoredElementIdentity.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return this.Equals((GroupChangedEventArgs)obj);
        }
    }
}
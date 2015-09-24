using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public enum ScreenType
    {
        Group = 0,
        Complete = 1
    }

    public class NavigationIdentity : EventArgs
    {
        public Identity TargetGroup { get; set; }

        public ScreenType ScreenType { get; set; }

        public Identity AnchoredElementIdentity { get; set; }

        public NavigationIdentity() { }

        public NavigationIdentity(Identity targetGroup, ScreenType screenType = ScreenType.Group, Identity anchoredElementIdentity = null)
            : this()
        {
            this.TargetGroup = targetGroup;
            this.ScreenType = screenType;
            this.AnchoredElementIdentity = anchoredElementIdentity;
        }

        protected bool Equals(NavigationIdentity other)
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
            return this.Equals((NavigationIdentity)obj);
        }
    }
}
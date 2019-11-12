using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public enum ScreenType
    {
        Group = 100,
        Complete = 1000,
        Cover = 5000,
        Identifying = 11000,
        Overview  = 100500,
        PdfViewByStaticText = 100600,
        PdfView = 100700
    }

    public class NavigationIdentity : EventArgs
    {
        public Identity TargetGroup { get; set; }

        public ScreenType TargetScreen { get; set; }

        public Identity AnchoredElementIdentity { get; set; }

        public NavigationIdentity() { }

        public static NavigationIdentity CreateForPrefieldScreen()
        {
            return new NavigationIdentity(ScreenType.Identifying, targetGroup: null);
        }

        public static NavigationIdentity CreateForCompleteScreen()
        {
            return new NavigationIdentity(ScreenType.Complete, targetGroup: null);
        }

        public static NavigationIdentity CreateForCoverScreen()
        {
            return new NavigationIdentity(ScreenType.Cover, targetGroup: null);
        }

        public static NavigationIdentity CreateForOverviewScreen()
        {
            return new NavigationIdentity(ScreenType.Overview, targetGroup: null);
        }

        public static NavigationIdentity CreateForGroup(Identity groupIdentity, Identity anchoredElementIdentity = null)
        {
            return new NavigationIdentity(ScreenType.Group, targetGroup: groupIdentity, anchoredElementIdentity: anchoredElementIdentity);
        }

        public static NavigationIdentity CreateForPdfViewByStaticText(Identity targetStaticText) =>
            new NavigationIdentity(ScreenType.PdfViewByStaticText, null, targetStaticText);
        public static NavigationIdentity CreateForPdfView(Guid attachmentId) =>
            new NavigationIdentity(ScreenType.PdfView, null, Identity.Create(attachmentId, RosterVector.Empty));

        private NavigationIdentity(ScreenType targetScreen, Identity targetGroup, Identity anchoredElementIdentity = null)
            : this()
        {
            this.TargetGroup = targetGroup;
            this.TargetScreen = targetScreen;
            this.AnchoredElementIdentity = anchoredElementIdentity;
        }

        protected bool Equals(NavigationIdentity other)
        {
            return Equals(this.TargetGroup, other.TargetGroup) 
                && Equals(this.AnchoredElementIdentity, other.AnchoredElementIdentity)
                && Equals(this.TargetScreen, other.TargetScreen);
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

        public override string ToString()
        {
            return $"{nameof(TargetGroup)}: {TargetGroup}, {nameof(TargetScreen)}: {TargetScreen}, {nameof(AnchoredElementIdentity)}: {AnchoredElementIdentity}";
        }
    }
}

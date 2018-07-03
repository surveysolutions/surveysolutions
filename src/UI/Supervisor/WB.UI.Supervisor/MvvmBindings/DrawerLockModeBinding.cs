using Android.Support.V4.Widget;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Supervisor.MvvmBindings
{
    public class DrawerLockModeBinding : BaseBinding<DrawerLayout, bool>
    {
        public DrawerLockModeBinding(DrawerLayout androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(DrawerLayout control, bool value) 
            => control.SetDrawerLockMode(value ? DrawerLayout.LockModeLockedClosed : DrawerLayout.LockModeUnlocked);
    }
}

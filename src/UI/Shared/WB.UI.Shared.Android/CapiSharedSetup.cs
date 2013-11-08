using Android.Content;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using WB.UI.Shared.Android.Bindings;

namespace WB.UI.Shared.Android
{
    public abstract class CapiSharedSetup : MvxAndroidSetup
    {
        public CapiSharedSetup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            registry.RegisterFactory(new MvxCustomBindingFactory<ViewGroup>("Background", (button) => new BackgroundBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("Html", (button) => new HtmlBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<View>("Visible", (button) => new VisibilityBinding(button)));
            registry.RegisterFactory(new MvxCustomBindingFactory<TextView>("ValidationMessage", (button) => new ValidationMessageBinding(button)));

            base.FillTargetFactories(registry);
        }
    }
}
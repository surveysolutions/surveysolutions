using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Droid.Support.V7.Fragging;
using MvvmCross.Droid.Support.V7.Fragging.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BaseFragment<TViewModel> : MvxFragment<TViewModel> where TViewModel : MvxViewModel
    {
        protected abstract int ViewResourceId { get; }

        /*        protected override void OnCreate(Bundle bundle)
                {
                    var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(ApplicationContext);
                    setup.EnsureInitialized();

                    base.OnCreate(bundle);
                }

                protected override void OnViewModelSet()
                {
                    base.OnViewModelSet();
                    this.SetContentView(this.ViewResourceId);
                }*/


        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(ViewResourceId, null);
        }

        /* protected override void OnCreate(Bundle bundle)
         {
             base.OnCreate(bundle);
             SetContentView(Resource.Layout.FirstView);

             var sub = (SubFrag)SupportFragmentManager.FindFragmentById(Resource.Id.sub1);
             sub.ViewModel = ((FirstViewModel)ViewModel).Sub;

             var dub = (DubFrag)SupportFragmentManager.FindFragmentById(Resource.Id.dub1);
             dub.ViewModel = ((FirstViewModel)ViewModel).Sub;
         }*/

        /*
                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    base.OnCreateView(inflater, container, savedInstanceState);
                    return this.BindingInflate(ViewResourceId, null);
                }*/
    }
}
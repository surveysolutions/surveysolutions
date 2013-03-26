using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.Binders;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Droid.ExtensionMethods;
using Cirrious.MvvmCross.Droid.Interfaces;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace CAPI.Android.Core
{
    public abstract class MvxSimpleBindingFragmentActivity<TViewModel> : FragmentActivity, IMvxBindingActivity, IMvxAndroidView<TViewModel>, IMvxServiceConsumer<IMvxIntentResultSink>
        where TViewModel : class, IMvxViewModel
    {
        protected MvxSimpleBindingFragmentActivity()
        {
            IsVisible = true;
        }

        protected override void OnCreate(Bundle bundle)
        {
            ClearAllBindings();
            base.OnCreate(bundle);
            
            this.OnViewCreate();
        }

        protected override void OnDestroy()
        {
            ClearAllBindings();
            this.OnViewDestroy();
            base.OnDestroy();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            this.OnViewNewIntent();
        }
        protected override void OnResume()
        {
            base.OnResume();
            IsVisible = true;
            this.OnViewResume();
        }

        protected override void OnPause()
        {
            this.OnViewPause();
            IsVisible = false;
            base.OnPause();
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.OnViewStart();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            this.OnViewRestart();
        }

        protected override void OnStop()
        {
            this.OnViewStop();
            base.OnStop();
        }
        public override void StartActivityForResult(Intent intent, int requestCode)
        {
            switch (requestCode)
            {
                case (int)MvxIntentRequestCode.PickFromFile:
                    Cirrious.MvvmCross.Platform.Diagnostics.MvxTrace.Trace("Warning - activity request code may clash with Mvx code for {0}", (MvxIntentRequestCode)requestCode);
                    break;
                default:
                    // ok...
                    break;
            }
            base.StartActivityForResult(intent, requestCode);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            this.GetService<IMvxIntentResultSink>().OnResult(new MvxIntentResultEventArgs(requestCode, resultCode, data));
            base.OnActivityResult(requestCode, resultCode, data);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ClearAllBindings();
            base.Dispose(disposing);
        }
        private View CommonInflate(int resourceId, ViewGroup viewGroup, Func<LayoutInflater, MvxBindingLayoutInflatorFactory> factoryProvider)
        {
            var layoutInflator = base.LayoutInflater;
            using (var clone = layoutInflator.CloneInContext(this))
            {
                using (var factory = factoryProvider(clone))
                {
                    if (factory != null)
                        clone.Factory = factory;
                    var toReturn = clone.Inflate(resourceId, viewGroup);
                    if (factory != null)
                    {
                        factory.StoreBindings(toReturn);
                    }
                    return toReturn;
                }
            }
        }
        private readonly List<View> _boundViews = new List<View>();
        private readonly List<IMvxBinding> _bindings = new List<IMvxBinding>();
        private void ClearAllBindings()
        {
            var cleaner = new MvxBindingLayoutCleaner();
            _boundViews.ForEach(cleaner.Clean);
            _boundViews.Clear();
            _bindings.ForEach(b => b.Dispose());
            _bindings.Clear();
        }
        #region Implementation of IMvxBindingActivity

        public void ClearBindings(View view)
        {
            if (view == null)
                return;

            var cleaner = new MvxBindingLayoutCleaner();
            cleaner.Clean(view);
            for (var i = 0; i < _boundViews.Count; i++)
            {
                if (_boundViews[i] == view)
                {
                    _boundViews.RemoveAt(i);
                    break;
                }
            }
        }

        public View BindingInflate(object source, int resourceId, ViewGroup viewGroup)
        {
            var view = CommonInflate(
                 resourceId,
                 viewGroup,
                 (layoutInflator) => new MvxBindingLayoutInflatorFactory(source, layoutInflator));
            RegisterBindingsFor(view);
            return view;
        }

        public View BindingInflate(int resourceId, ViewGroup viewGroup)
        {
            return CommonInflate(
                 resourceId,
                 viewGroup,
                 (layoutInflator) => null);
        }

        public View NonBindingInflate(int resourceId, ViewGroup viewGroup)
        {
            return CommonInflate(
                  resourceId,
                  viewGroup,
                  (layoutInflator) => null);
        }

        public void RegisterBindingsFor(View view)
        {
            if (view == null)
                return;

            _boundViews.Add(view);
        }

        public void RegisterBinding(IMvxBinding binding)
        {
            _bindings.Add(binding);
        }
        #endregion

        #region Implementation of IMvxView

        public bool IsVisible { get; private set; }
        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            base.StartActivityForResult(intent, requestCode);
        }

        #endregion

        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnViewModelSet();
            }
        }
        private TViewModel _viewModel;

        protected virtual void OnViewModelSet()
        {
        }

        public override void SetContentView(int layoutResId)
        {
            this.SetContentView(this.BindingInflate(layoutResId, (ViewGroup)null));
        }
    }
}
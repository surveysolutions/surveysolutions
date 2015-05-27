using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Droid.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace WB.UI.QuestionnaireTester.Activities
{
    public class BaseMvxActivity : AppCompatActivity, IMvxEventSourceActivity, IMvxAndroidView
    {
        protected BaseMvxActivity()
        {
            this.BindingContext = new MvxAndroidBindingContext(this, this);
            this.AddEventListeners();
        }

        public event EventHandler<MvxValueEventArgs<MvxActivityResultParameters>> ActivityResultCalled;
        public event EventHandler<MvxValueEventArgs<Bundle>> CreateCalled;
        public event EventHandler<MvxValueEventArgs<Bundle>> CreateWillBeCalled;
        public event EventHandler DestroyCalled;
        public event EventHandler DisposeCalled;
        public event EventHandler<MvxValueEventArgs<Intent>> NewIntentCalled;
        public event EventHandler PauseCalled;
        public event EventHandler RestartCalled;
        public event EventHandler ResumeCalled;
        public event EventHandler<MvxValueEventArgs<Bundle>> SaveInstanceStateCalled;
        public event EventHandler<MvxValueEventArgs<MvxStartActivityForResultParameters>> StartActivityForResultCalled;
        public event EventHandler StartCalled;
        public event EventHandler StopCalled;

        public IMvxBindingContext BindingContext { get; set; }

        public object DataContext
        {
            get { return this.BindingContext.DataContext; }
            set { this.BindingContext.DataContext = value; }
        }

        public IMvxViewModel ViewModel
        {
            get { return this.DataContext as IMvxViewModel; }
            set
            {
                this.DataContext = value;
                this.OnViewModelSet();
            }
        }

        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            base.StartActivityForResult(intent, requestCode);
        }

        public override void SetContentView(int layoutResId)
        {
            var view = this.BindingInflate(layoutResId, null);
            this.SetContentView(view);
        }

        public override void StartActivityForResult(Intent intent, int requestCode)
        {
            this.StartActivityForResultCalled.Raise(this, new MvxStartActivityForResultParameters(intent, requestCode));
            base.StartActivityForResult(intent, requestCode);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeCalled.Raise(this);
            }
            base.Dispose(disposing);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            this.ActivityResultCalled.Raise(this, new MvxActivityResultParameters(requestCode, resultCode, data));
            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected override void OnCreate(Bundle bundle)
        {
            this.CreateWillBeCalled.Raise(this, bundle);
            base.OnCreate(bundle);
            this.CreateCalled.Raise(this, bundle);
        }

        protected override void OnDestroy()
        {
            this.DestroyCalled.Raise(this);
            base.OnDestroy();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            this.NewIntentCalled.Raise(this, intent);
        }

        protected override void OnPause()
        {
            this.PauseCalled.Raise(this);
            base.OnPause();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            this.RestartCalled.Raise(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.ResumeCalled.Raise(this);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            this.SaveInstanceStateCalled.Raise(this, outState);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.StartCalled.Raise(this);
        }

        protected override void OnStop()
        {
            this.StopCalled.Raise(this);
            base.OnStop();
        }

        protected virtual void OnViewModelSet()
        {
        }
    }
}
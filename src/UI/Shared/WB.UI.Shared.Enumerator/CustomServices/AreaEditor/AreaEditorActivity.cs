using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Binding.BindingContext;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class AreaEditorActivity : BaseActivity<AreaEditorViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_area_editor;

        public static event Action<AreaEditorResult> OnAreaEditCompleted;

        public override void OnBackPressed()
        {
            this.Cancel();
        }

        private void Cancel()
        {
            var handler = OnAreaEditCompleted;
            handler?.Invoke((AreaEditorResult)null);
            this.Finish();
        }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.ViewModel.OnAreaEditCompleted += OnAreaEditCompleted;
            
            //esri bug
            //inflated map doesn't work  
            var container = this.FindViewById<LinearLayout>(Resource.Id.AreaMapViewContainer);

            var map = new MapView();

            var bindingSet = this.CreateBindingSet<AreaEditorActivity, AreaEditorViewModel>();

            bindingSet.Bind(map)
                .For(v => v.Map)
                .To(vm => vm.Map);

            bindingSet.Apply();

            this.ViewModel.MapView = map;

            container.AddView(map);
        }
        
    }
}
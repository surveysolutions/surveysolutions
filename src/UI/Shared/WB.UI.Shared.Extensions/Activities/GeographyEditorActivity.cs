using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.ViewModels;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/AppTheme", 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
    public class GeographyEditorActivity : BaseActivity<GeographyEditorViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_area_editor;

        public static Action<AreaEditorResult> OnAreaEditCompleted;

        public override void OnBackPressed()
        {
            this.Cancel();
        }

        private void Cancel()
        {
            var handler = OnAreaEditCompleted;
            handler?.Invoke(null);
            this.Finish();
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);

            this.ViewModel.OnAreaEditCompleted = OnAreaEditCompleted;

            //workaround
            //inflated map doesn't work (should be fixed in next Esri release) 
            var mapView = this.ViewModel.MapView;
            var bindingSet = this.CreateBindingSet<GeographyEditorActivity, GeographyEditorViewModel>();

            bindingSet.Bind(mapView)
                .For(v => v.Map)
                .To(vm => vm.Map);

            bindingSet.Apply();
            
            var container = this.FindViewById<LinearLayout>(Resource.Id.area_map_view_container);
            container.AddView(mapView);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.area_editor, menu);

            menu.LocalizeMenuItem(Resource.Id.map_editor_exit, UIResources.MenuItem_Title_AreaCancelEdit);

            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Resource.Id.map_editor_exit)
                this.ViewModel.CancelCommand.Execute();
            
            return base.OnOptionsItemSelected(item);
        }
    }
}

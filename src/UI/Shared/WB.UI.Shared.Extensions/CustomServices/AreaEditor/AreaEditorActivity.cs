using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.UI.Controls;
using MvvmCross.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Shared.Extensions.CustomServices.AreaEditor
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/AppTheme", 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
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
            handler?.Invoke(null);
            this.Finish();
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);

            this.ViewModel.OnAreaEditCompleted += OnAreaEditCompleted;

            //workaround
            //inflated map doesn't work (should be fixed in next Esri release) 
            var map = new MapView();
            var bindingSet = this.CreateBindingSet<AreaEditorActivity, AreaEditorViewModel>();

            bindingSet.Bind(map)
                .For(v => v.Map)
                .To(vm => vm.Map);

            bindingSet.Apply();

            this.ViewModel.MapView = map;

            var container = this.FindViewById<LinearLayout>(Resource.Id.area_map_view_container);
            container.AddView(map);
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

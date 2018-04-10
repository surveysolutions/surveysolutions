using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/AppTheme", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class PhotoViewActivity : BaseActivity<PhotoViewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_photo_view;
        
        public override void OnBackPressed()
        {
            this.Cancel();
        }

        private void Cancel()
        {
            this.Finish();
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //var toolbar = this.FindViewById<Toolbar>(Android.Resource.Id.toolbar);
            //toolbar.Title = "";
            //this.SetSupportActionBar(toolbar);

            //this.ViewModel.OnAreaEditCompleted += OnAreaEditCompleted;

            //workaround
            //inflated map doesn't work (should be fixed in next Esri release) 
            /*var map = new MapView();
            var bindingSet = this.CreateBindingSet<AreaEditorActivity, AreaEditorViewModel>();

            bindingSet.Bind(map)
                .For(v => v.Map)
                .To(vm => vm.Map);

            bindingSet.Apply();

            this.ViewModel.MapView = map;

            var container = this.FindViewById<LinearLayout>(Android.Resource.Id.area_map_view_container);
            container.AddView(map);*/
        }

        /*public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Android.Resource.Menu.area_editor, menu);

            menu.LocalizeMenuItem(Android.Resource.Id.map_editor_exit, UIResources.MenuItem_Title_AreaCancelEdit);

            return base.OnCreateOptionsMenu(menu);
        }*/
        /*public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if(item.ItemId == Android.Resource.Id.map_editor_exit)
                this.ViewModel.CancelCommand.Execute();
            
            return base.OnOptionsItemSelected(item);
        }*/

    }
}

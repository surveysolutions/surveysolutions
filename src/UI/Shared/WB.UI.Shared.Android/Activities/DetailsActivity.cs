using System;
using System.Linq;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Adapters;
using WB.UI.Shared.Android.Controls;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Activities
{
    public abstract class DetailsActivity : DoubleBackMvxFragmentActivity
    {
        protected Guid QuestionnaireId
        {
            get { return Guid.Parse(this.Intent.GetStringExtra("publicKey")); }
        }

        protected InterviewViewModel Model
        {
            get { return this.ViewModel as InterviewViewModel; }
        }

        protected ViewPager VpContent
        {
            get { return this.FindViewById<ViewPager>(Resource.Id.vpContent); }
        }

        protected LinearLayout llNavigationHolder
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llNavigationHolder); }
        }

        protected DrawerLayout llContainer
        {
            get { return this.FindViewById<DrawerLayout>(Resource.Id.llContainer); }
        }

        private ContentFrameAdapter adapter;
        private QuestionnaireNavigationView navList;
        private InterviewItemId? screenId;
        private ActionBarDrawerToggle drawerToggle;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.Window.SetSoftInputMode(SoftInput.AdjustPan);

            this.ViewModel = this.GetInterviewViewModel(this.QuestionnaireId);

            SetContentView(Resource.Layout.Details);

            if (bundle != null)
            {
                var savedScreen = bundle.GetString("ScreenId");
                if (!string.IsNullOrEmpty(savedScreen))
                {
                    this.screenId = InterviewItemId.Parse(savedScreen);
                }
            }
            else
            {
                this.screenId = this.Model.Chapters.FirstOrDefault().ScreenId;
            }

            if (bundle == null)
            {
                this.navList = new QuestionnaireNavigationView(this, this.Model);
                this.llNavigationHolder.AddView(this.navList);
                this.navList.ScreenChanged += this.ContentFrameAdapterScreenChanged;
            }
            else
            {
                this.navList = this.llNavigationHolder.GetChildAt(0) as QuestionnaireNavigationView;
            }
            Title = CreateScreenTitle();
            this.adapter = this.CreateFrameAdapter(this.screenId);
            this.VpContent.Adapter = this.adapter;
            this.VpContent.PageSelected += this.VpContentPageSelected;
            
            this.drawerToggle = new ActionBarDrawerToggle(this, this.llContainer, Android.Resource.Drawable.ic_drawer_dark,
                Resource.String.drawer_open,
                Resource.String.drawer_close);

            //Set the drawer lister to be the toggle.
            this.llContainer.SetDrawerListener(this.drawerToggle);
        }

        protected override void OnPostCreate(Bundle savedInstanceState)
        {
            this.ActionBar.SetDisplayHomeAsUpEnabled(true);
            base.OnPostCreate(savedInstanceState);
            this.drawerToggle.SyncState();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            this.drawerToggle.OnConfigurationChanged(newConfig);
        }

        // Pass the event to ActionBarDrawerToggle, if it returns
        // true, then it has handled the app icon touch event
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (this.drawerToggle.OnOptionsItemSelected(item))
                return true;

            return base.OnOptionsItemSelected(item);
        }

        protected abstract ContentFrameAdapter CreateFrameAdapter(InterviewItemId? screenId);
        protected abstract InterviewViewModel GetInterviewViewModel(Guid interviewId);


        protected virtual string CreateScreenTitle()
        {
            return string.Format("{0} {1}", this.Model.Title,
                string.Join("|",
                    this.Model.FeaturedQuestions.Values.Where(q => !string.IsNullOrEmpty(q.AnswerString)).Select(q => q.AnswerString)));
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            if (!this.adapter.ScreenId.HasValue)
                return;
            outState.PutString("ScreenId", this.adapter.ScreenId.Value.ToString());
        }

        public override void OnAttachFragment(global::Android.Support.V4.App.Fragment p0)
        {
            var screen = p0 as IScreenChanging;
            if (screen != null)
            {
                screen.ScreenChanged += this.ContentFrameAdapterScreenChanged;
            }
            base.OnAttachFragment(p0);
        }

        private void ContentFrameAdapterScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            var index = this.adapter.GetScreenIndex(e.ScreenId);

            if (index >= 0)
            {
                this.VpContent.CurrentItem = this.adapter.GetScreenIndex(e.ScreenId);
                this.adapter.NotifyDataSetChanged();

                this.llContainer.CloseDrawers();
                return;
            }
            this.adapter.UpdateScreenData(e.ScreenId);
            this.adapter.NotifyDataSetChanged();

            this.VpContent.CurrentItem = this.adapter.GetScreenIndex(e.ScreenId);

            if (e.ScreenId.HasValue)
            {
                var screen = this.Model.Screens[ConversionHelper.ConvertIdAndRosterVectorToString(e.ScreenId.Value.Id, e.ScreenId.Value.InterviewItemPropagationVector)];
                var chapterKey = screen.Breadcrumbs.First();
                for (int i = 0; i < this.Model.Chapters.Count; i++)
                {
                    if (this.Model.Chapters[i].ScreenId == chapterKey)
                    {
                        this.navList.SelectItem(i);
                    }
                }
            }
            this.llContainer.CloseDrawers();
            GC.Collect(0);

        }

        private void VpContentPageSelected(object sender, ViewPager.PageSelectedEventArgs e)
        {
            if (this.adapter.IsRoot)
                this.navList.SelectItem(e.Position);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(this.VpContent != null)
                this.VpContent.PageSelected -= this.VpContentPageSelected;
            if(this.navList != null)
                this.navList.ScreenChanged -= this.ContentFrameAdapterScreenChanged;

            GC.Collect();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}
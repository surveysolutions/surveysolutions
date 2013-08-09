using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace CAPI.Android.Controls.QuestionnaireDetails.Roster
{
    public class RosterQuestionView : LinearLayout
    {
        private readonly IMvxAndroidBindingContext bindingActivity;
        protected QuestionViewModel Model { get; private set; }
        protected View Content { get; set; }

        protected LinearLayout llWrapper
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llWrapper); }
        }

        public RosterQuestionView(Context context,  QuestionViewModel source)
            : base(context)
        {
            bindingActivity = new MvxAndroidBindingContext(context, ((context as IMvxBindingContextOwner).BindingContext as IMvxAndroidBindingContext).LayoutInflater, source);
            this.Model = source;
            Content = bindingActivity.BindingInflate(Resource.Layout.RosterQuestion, this);
           
          
            llWrapper.Click += rowViewItem_Click;
        }
        protected override void OnAttachedToWindow()
        {
            llWrapper.EnableDisableView(this.Model.Status.HasFlag(QuestionStatus.Enabled));
            base.OnAttachedToWindow();
        }
        void rowViewItem_Click(object sender, EventArgs e)
        {
            var handler = RosterItemsClick;
            if (handler != null)
            {
                handler(this, new RosterItemClickEventArgs(Model));
            }
        }

        public event EventHandler<RosterItemClickEventArgs> RosterItemsClick;


        public void ClearBindings()
        {
            bindingActivity.ClearBindings(this);
        }

        protected IMvxAndroidBindingContext BindingActivity
        {
            get { return bindingActivity; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearBindings();
            }

            base.Dispose(disposing);
        }
    }
}

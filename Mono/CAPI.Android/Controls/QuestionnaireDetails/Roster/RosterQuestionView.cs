using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;

namespace CAPI.Android.Controls.QuestionnaireDetails.Roster
{
    public class RosterQuestionView : LinearLayout
    {
        private readonly IMvxBindingActivity _bindingActivity;
        protected QuestionViewModel Model { get; private set; }
        protected View Content { get; set; }

        protected LinearLayout llWrapper
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llWrapper); }
        }

        public RosterQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source)
            : base(context)
        {
            _bindingActivity = bindingActivity;
            this.Model = source;
            Content = bindingActivity.BindingInflate(source, Resource.Layout.RosterQuestion, this);
           
            llWrapper.EnableDisableView(this.Model.Status.HasFlag(QuestionStatus.Enabled));
            llWrapper.Click += rowViewItem_Click;
         //   this.SetBackgroundResource(Resource.Drawable.grid_headerItem);
        }

        void rowViewItem_Click(object sender, EventArgs e)
        {
            if (!this.Model.Status.HasFlag(QuestionStatus.Enabled))
                return;
            var handler = RosterItemsClick;
            if (handler != null)
            {
                handler(this, new RosterItemClickEventArgs(Model));
            }
            //   var template = Model.Header[i];
        }

        public event EventHandler<RosterItemClickEventArgs> RosterItemsClick;


        public void ClearBindings()
        {
            _bindingActivity.ClearBindings(this);
        }

        protected IMvxBindingActivity BindingActivity
        {
            get { return _bindingActivity; }
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

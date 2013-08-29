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
    public class RosterQuestionView : LinearLayout, IMvxBindingContextOwner
    {
        private readonly IMvxAndroidBindingContext _bindingContext;
        protected QuestionViewModel Model { get; private set; }
        protected View Content { get; set; }

        protected LinearLayout llWrapper
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llWrapper); }
        }


        public IMvxBindingContext BindingContext
        {
            get { return _bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the roster view"); }
        }

        public RosterQuestionView(Context context,  QuestionViewModel source)
            : base(context)
        {
            _bindingContext = new MvxAndroidBindingContext(context, ((context as IMvxBindingContextOwner).BindingContext as IMvxAndroidBindingContext).LayoutInflater, source);
            this.Model = source;
            Content = _bindingContext.BindingInflate(Resource.Layout.RosterQuestion, this);
           
          
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


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearAllBindings();
            }

            base.Dispose(disposing);
        }
    }
}

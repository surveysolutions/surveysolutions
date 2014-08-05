using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class StaticTextView : LinearLayout, IMvxBindingContextOwner
    {
        private MvxAndroidBindingContext bindingContext;
        protected View Content { get; set; }
        public StaticTextViewModel Model { get; set; }

        public StaticTextView(Context context, StaticTextViewModel source)
            : base(context)
        {
            this.bindingContext = new MvxAndroidBindingContext(context, context.ToBindingContext().LayoutInflater, source);
            this.Content = this.bindingContext.BindingInflate(Resource.Layout.StaticTextView, this);
            this.Model = source;
        }

        public IMvxBindingContext BindingContext
        {
            get { return this.bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the static text"); }
        }
    }
} ;
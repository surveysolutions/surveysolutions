using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using WB.Core.BoundedContexts.Capi.ViewModel;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using InterviewViewModel = WB.Core.BoundedContexts.Capi.ViewModel.InterviewViewModel;

namespace AxmlTester.Droid
{
    [Activity(Label = "AxmlTester.Droid", MainLauncher = true, Icon = "@drawable/icon", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class MainActivity : BaseMvxActivity<InterviewViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Questions);

            var list = FindViewById<MvxListView>(Resource.Id.TheListView);
            list.Adapter = new CustomAdapter(this, (IMvxAndroidBindingContext)BindingContext);

        }
    }

    public class BaseMvxActivity<T> : MvxActivity where T:IMvxViewModel
    {
        public new T ViewModel
        {
            get { return (T) base.ViewModel; }
            set { base.ViewModel = value; }
        }
    }

    public class CustomAdapter : MvxAdapter
    {
        public CustomAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

        public override int GetItemViewType(int position)
        {
            var item = GetRawItem(position);
            if (item is InterviewStaticText)
                return 0;
            else if (item is InterviewTextQuestion)
                return 1;
            else if (item is InterviewDateQuestion)
                return 2;
            else if (item is InterviewDecimalQuestion)
                return 3;
            else if (item is InterviewImageQuestion)
                return 4;
            else if (item is InterviewIntegerQuestion)
                return 5;
            else if (item is InterviewListQuestion)
                return 6;
            else if (item is InterviewQrBarcodeQuestion)
                return 7;

            return 0;
        }

        public override int ViewTypeCount
        {
            get { return 8; }
        }

        protected override View GetBindableView(View convertView, object source, int templateId)
        {
            if (source is InterviewStaticText)
                templateId = Resource.Layout.StaticText;
            else if (source is InterviewTextQuestion)
                templateId = Resource.Layout.TextQuestion;
            else if (source is InterviewDateQuestion)
                templateId = Resource.Layout.InterviewDateQuestion;
            else if (source is InterviewDecimalQuestion)
                templateId = Resource.Layout.InterviewDecimalQuestion;
            else if (source is InterviewImageQuestion)
                templateId = Resource.Layout.InterviewImageQuestion;
            else if (source is InterviewIntegerQuestion)
                templateId = Resource.Layout.InterviewIntegerQuestion;
            else if (source is InterviewListQuestion)
                templateId = Resource.Layout.InterviewListQuestion;
            else if (source is InterviewQrBarcodeQuestion)
                templateId = Resource.Layout.InterviewQrBarcodeQuestion;

            return base.GetBindableView(convertView, source, templateId);
        }
    }

}


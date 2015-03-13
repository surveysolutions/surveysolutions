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

        static Dictionary<Type, int> questionTemplates = new Dictionary<Type, int>()
        {
             { typeof(InterviewStaticText), Resource.Layout.InterviewStaticText },
             { typeof(InterviewTextQuestion), Resource.Layout.InterviewTextQuestion },
             { typeof(InterviewDateQuestion), Resource.Layout.InterviewDateQuestion },
             { typeof(InterviewDecimalQuestion), Resource.Layout.InterviewDecimalQuestion },
             { typeof(InterviewImageQuestion), Resource.Layout.InterviewImageQuestion },
             { typeof(InterviewIntegerQuestion), Resource.Layout.InterviewIntegerQuestion },
             { typeof(InterviewMultiChoiceQuestion), Resource.Layout.InterviewMultiChoiceQuestion },
             { typeof(InterviewSingleChoiceQuestion), Resource.Layout.InterviewSingleChoiceQuestion },
             { typeof(InterviewGroup), Resource.Layout.InterviewGroup },
        }; 

        public override int GetItemViewType(int position)
        {
            var item = GetRawItem(position);

            int index = 0;
            foreach (var pair in questionTemplates)
            {
                if (item.GetType().IsInstanceOfType(pair.Key))
                    return index;

                index++;
            }

            return 0;
        }

        public override int ViewTypeCount
        {
            get { return questionTemplates.Count; }
        }

        protected override View GetBindableView(View convertView, object source, int templateId)
        {
            Type type = source.GetType();
            if (questionTemplates.ContainsKey(type))
                templateId = questionTemplates[type];

            return base.GetBindableView(convertView, source, templateId);
        }
    }
}


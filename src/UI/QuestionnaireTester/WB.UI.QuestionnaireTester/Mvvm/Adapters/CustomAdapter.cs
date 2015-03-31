using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.QuestionnaireTester.Mvvm.Adapters
{
    /*How to use 
     var list = FindViewById<MvxListView>(Resource.Id.TheListView);
            list.Adapter = new CustomAdapter(this, (IMvxAndroidBindingContext)BindingContext);*/

    public class CustomAdapter : MvxAdapter
    {
        public CustomAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

        static Dictionary<Type, int> questionTemplates = new Dictionary<Type, int>()
        {
            //{ typeof(InterviewStaticText), Resource.Layout.InterviewStaticText },
            //{ typeof(InterviewTextQuestion), Resource.Layout.InterviewTextQuestion },
            //{ typeof(InterviewDateQuestion), Resource.Layout.InterviewDateQuestion },
            //{ typeof(InterviewDecimalQuestion), Resource.Layout.InterviewDecimalQuestion },
            //{ typeof(InterviewImageQuestion), Resource.Layout.InterviewImageQuestion },
            //{ typeof(InterviewIntegerQuestion), Resource.Layout.InterviewIntegerQuestion },
            //{ typeof(InterviewMultiChoiceQuestion), Resource.Layout.InterviewMultiChoiceQuestion },
            //{ typeof(InterviewSingleChoiceQuestion), Resource.Layout.InterviewSingleChoiceQuestion },
            //{ typeof(InterviewGroup), Resource.Layout.InterviewGroup },
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
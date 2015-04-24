using System.Collections.Generic;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Android.Content;
using System;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.UI.QuestionnaireTester.Views.CustomControls;


namespace WB.UI.QuestionnaireTester.Views.Adapters
{
    public class InterviewEntityAdapter : MvxRecyclerViewAdapter
    {
        public InterviewEntityAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }
       
        private static readonly Dictionary<Type, int> QuestionTemplates = new Dictionary<Type, int>
        {
            {typeof (StaticTextViewModel), Resource.Layout.interview_static_text },
            {typeof (MaskedTextQuestionViewModel), Resource.Layout.interview_text_question },
            {typeof (GpsCoordinatesQuestionViewModel), Resource.Layout.interview_gps_question },
        };

        public override int GetItemViewType(int position)
        {
            var source = this.GetRawItem(position);

            return QuestionTemplates[source.GetType()];
        }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return bindingContext.BindingInflate(viewType, parent, false);
        }
    }
}


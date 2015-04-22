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
            {typeof (GroupReferenceViewModel), Resource.Layout.interview_group_reference },
            {typeof (RostersReferenceViewModel), Resource.Layout.interview_roster_reference },

            {typeof (SingleOptionQuestionViewModel), Resource.Layout.interview_single_option_question },
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_date_question },
            {typeof (MaskedTextQuestionViewModel), Resource.Layout.interview_text_question },
            {typeof (RealNumericQuestionViewModel), Resource.Layout.interview_decimal_question },
            {typeof (MultimediaQuestionViewModel), Resource.Layout.interview_image_question },
            {typeof (IntegerNumericQuestionViewModel), Resource.Layout.interview_integer_question },
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_multi_option_question },
            {typeof (LinkedSingleOptionQuestionViewModel), Resource.Layout.interview_linked_single_question },
            {typeof (LinkedMultiOptionQuestionViewModel), Resource.Layout.interview_linked_multi_question },
            {typeof (TextListQuestionViewModel), Resource.Layout.interview_text_list_question },
            {typeof (QrBarcodeQuestionViewModel), Resource.Layout.interview_qr_question },
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


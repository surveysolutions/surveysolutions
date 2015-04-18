using System.Collections.Generic;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using System;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;


namespace WB.UI.QuestionnaireTester.Views.Adapters
{
    public class QuestionAdapter : MvxAdapter
    {
        public QuestionAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

       
        private static readonly Dictionary<Type, int> QuestionTemplates = new Dictionary<Type, int>
        {
            {typeof (StaticTextViewModel), Resource.Layout.interview_static_text },
            {typeof (GroupReferenceViewModel), Resource.Layout.interview_group_reference },

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
            var item = GetRawItem(position);

            int index = 0;
            foreach (var pair in QuestionTemplates)
            {
                if (item.GetType().IsInstanceOfType(pair.Key))
                    return index;

                index++;
            }

            return 0;
        }

        public override int ViewTypeCount
        {
            get { return QuestionTemplates.Count; }
        }

        protected override View GetBindableView(View convertView, object source, int templateId)
        {
            Type type = source.GetType();
            if (QuestionTemplates.ContainsKey(type))
                templateId = QuestionTemplates[type];

            return base.GetBindableView(convertView, source, templateId);
        }
    }
}


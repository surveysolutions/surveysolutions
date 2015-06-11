using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using GroupViewModel = WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels.GroupViewModel;

namespace WB.UI.QuestionnaireTester.CustomControls
{
    public class InterviewEntityAdapter : MvxRecyclerViewAdapter
    {
        private const int UnknownViewType = -1;

        public InterviewEntityAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }

        private static readonly Dictionary<Type, int> QuestionTemplates = new Dictionary<Type, int>
        {
            {typeof (StaticTextViewModel), Resource.Layout.interview_static_text},
            {typeof (TextListQuestionViewModel), Resource.Layout.interview_question_text_list},
            {typeof (TextQuestionViewModel), Resource.Layout.interview_question_text},
            {typeof (IntegerQuestionViewModel), Resource.Layout.interview_question_integer},
            {typeof (RealQuestionViewModel), Resource.Layout.interview_question_real},
            {typeof (GpsCoordinatesQuestionViewModel), Resource.Layout.interview_question_gps},
            {typeof (MultimedaQuestionViewModel), Resource.Layout.interview_question_multimedia},
            {typeof (SingleOptionQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (LinkedMultiOptionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_question_datetime},
            {typeof (FilteredComboboxQuestionViewModel), Resource.Layout.interview_question_filter_combobox},
            {typeof (QRBarcodeQuestionViewModel), Resource.Layout.interview_question_qrbarcode},
            {typeof (GroupViewModel), Resource.Layout.interview_group},
            {typeof (RosterViewModel), Resource.Layout.interview_roster},
            {typeof (PreviousGroupNavigationViewModel), Resource.Layout.interview__previous_group_navigation},
            {typeof (StartInterviewViewModel), Resource.Layout.prefilled_questions_start_button},
        };

        public override int GetItemViewType(int position)
        {
            object source = this.GetRawItem(position);

            var typeOfViewModel = source.GetType();

            return QuestionTemplates.ContainsKey(typeOfViewModel) ?  QuestionTemplates[typeOfViewModel] : UnknownViewType;
        }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return viewType != UnknownViewType
                ? bindingContext.BindingInflate(viewType, parent, false)
                : this.CreateEmptyView();
        }

        private View CreateEmptyView()
        {
            return new View(this.Context);
        }
    }
}
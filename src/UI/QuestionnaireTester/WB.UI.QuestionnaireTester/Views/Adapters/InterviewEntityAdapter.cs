using System.Collections.Generic;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Android.Content;
using System;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.UI.QuestionnaireTester.Views.CustomControls;
using GroupViewModel = WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels.GroupViewModel;

namespace WB.UI.QuestionnaireTester.Views.Adapters
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
            {typeof (TextQuestionViewModel), Resource.Layout.interview_question_text},
            {typeof (IntegerQuestionViewModel), Resource.Layout.interview_question_integer},
            {typeof (RealQuestionViewModel), Resource.Layout.interview_question_real},
            {typeof (GpsCoordinatesQuestionViewModel), Resource.Layout.interview_question_gps},
            {typeof (SingleOptionQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_question_datetime},
            {typeof (QrBarcodeQuestionViewModel), Resource.Layout.interview_question_qrbarcode},
            {typeof (GroupViewModel), Resource.Layout.interview_group},
            {typeof (RosterViewModel), Resource.Layout.interview_roster},
            {typeof (PreviousGroupNavigationViewModel), Resource.Layout.interview__previous_group_navigation},
        };

        public override int GetItemViewType(int position)
        {
            var source = this.GetRawItem(position);

            var typeOfViewModel = source.GetType();

            return QuestionTemplates.ContainsKey(typeOfViewModel) ?  QuestionTemplates[typeOfViewModel] : UnknownViewType;
        }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return viewType != UnknownViewType
                ? bindingContext.BindingInflate(viewType, parent, false)
                : CreateEmptyView();
        }

        private View CreateEmptyView()
        {
            return new View(this.Context);
        }
    }
}
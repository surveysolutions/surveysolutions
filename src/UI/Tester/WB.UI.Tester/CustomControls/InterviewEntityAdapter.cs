using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Microsoft.CSharp.RuntimeBinder;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.BoundedContexts.Tester.ViewModels.Groups;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;

using GroupViewModel = WB.Core.BoundedContexts.Tester.ViewModels.Groups.GroupViewModel;

namespace WB.UI.Tester.CustomControls
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
            {typeof (SingleOptionLinkedQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (MultiOptionLinkedQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_question_datetime},
            {typeof (FilteredSingleOptionQuestionViewModel), Resource.Layout.interview_question_filtered_single_option },
            {typeof (CascadingSingleOptionQuestionViewModel), Resource.Layout.interview_question_cascading_single_option },
            {typeof (QRBarcodeQuestionViewModel), Resource.Layout.interview_question_qrbarcode},
            {typeof (GroupViewModel), Resource.Layout.interview_group},
            {typeof (RosterViewModel), Resource.Layout.interview_roster},
            {typeof (GroupNavigationViewModel), Resource.Layout.interview_group_navigation},
            {typeof (StartInterviewViewModel), Resource.Layout.prefilled_questions_start_button},
        };

        public override int GetItemViewType(int position)
        {
            object source = this.GetRawItem(position);

            var typeOfViewModel = source.GetType();

            if (typeOfViewModel.Name.EndsWith("QuestionViewModel"))
            {
                var enablementModel = GetEnablementViewModel(source);
                if (enablementModel != null && !enablementModel.Enabled)
                {
                    return Resource.Layout.interview_disabled_question;
                }
            }

            return QuestionTemplates.ContainsKey(typeOfViewModel) ?  QuestionTemplates[typeOfViewModel] : UnknownViewType;
        }

        private EnablementViewModel GetEnablementViewModel(dynamic item)
        {
            try
            {
                var enablementModel = item.QuestionState.Enablement;
                return enablementModel as EnablementViewModel;
            }
            catch (RuntimeBinderException)
            {
                
            }
            return null;
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
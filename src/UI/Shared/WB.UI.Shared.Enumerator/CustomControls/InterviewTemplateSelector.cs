using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class InterviewTemplateSelector : IMvxTemplateSelector
    {
        private static readonly ConcurrentDictionary<Type, bool> hasEnablementViewModel = new ConcurrentDictionary<Type, bool>();
        private const int UnknownViewType = -1;

        private static readonly Dictionary<Type, int> EntityTemplates = new Dictionary<Type, int>
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
            {typeof (SingleOptionRosterLinkedQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (MultiOptionLinkedToQuestionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (MultiOptionLinkedToRosterQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_question_datetime},
            {typeof (TimestampQuestionViewModel), Resource.Layout.interview_question_timestamp},
            {typeof (FilteredSingleOptionQuestionViewModel), Resource.Layout.interview_question_filtered_single_option },
            {typeof (CascadingSingleOptionQuestionViewModel), Resource.Layout.interview_question_cascading_single_option },
            {typeof (QRBarcodeQuestionViewModel), Resource.Layout.interview_question_qrbarcode},
            {typeof (YesNoQuestionViewModel), Resource.Layout.interview_question_yesno},
            {typeof (GroupViewModel), Resource.Layout.interview_group},
            {typeof (GroupNavigationViewModel), Resource.Layout.interview_group_navigation},
            {typeof (StartInterviewViewModel), Resource.Layout.prefilled_questions_start_button},
            {typeof (CompleteInterviewViewModel), Resource.Layout.interview_complete_status_change},

            {typeof (MultiOptionQuestionOptionViewModel), Resource.Layout.interview_question_multi_option_item},
            {typeof (SingleOptionQuestionOptionViewModel), Resource.Layout.interview_question_single_option_item},
            {typeof (QuestionHeaderViewModel), Resource.Layout.interview_question_header},
            {typeof (ValidityViewModel), Resource.Layout.interview_question_validation},
            {typeof (CommentsViewModel), Resource.Layout.interview_question_comments},

        };

        public int GetItemViewType(object forItemObject)
        {
            object source = forItemObject;

            var typeOfViewModel = source.GetType();

            if (typeOfViewModel.IsGenericType )
            {
                if (typeOfViewModel.GetGenericTypeDefinition() == typeof(OptionTopBorderViewModel<>))
                {
                    return Resource.Layout.interview_question_option_top_rounded_corner;
                }
                if (typeOfViewModel.GetGenericTypeDefinition() == typeof(OptionBottomBorderViewModel<>))
                {
                    return Resource.Layout.interview_question_option_bottom_rounded_corner;
                }
            }

            var enablementModel = this.GetEnablementViewModel(source);
            if (typeOfViewModel.Name.EndsWith("QuestionViewModel"))
            {
                if (enablementModel != null && !enablementModel.Enabled)
                {
                    return Resource.Layout.interview_disabled_question;
                }
            }

            if (typeOfViewModel == typeof(GroupViewModel))
            {
                if (enablementModel != null && !enablementModel.Enabled)
                {
                    return Resource.Layout.interview_disabled_group;
                }
            }

            if (typeOfViewModel == typeof(StaticTextViewModel))
            {
                if (enablementModel != null && !enablementModel.Enabled)
                {
                    return Resource.Layout.interview_disabled_static_text;
                }
            }

            return EntityTemplates.ContainsKey(typeOfViewModel)
                ? EntityTemplates[typeOfViewModel]
                : EntityTemplates.ContainsKey(typeOfViewModel.BaseType) ? EntityTemplates[typeOfViewModel.BaseType] : UnknownViewType;
        }

        private EnablementViewModel GetEnablementViewModel(dynamic item)
        {
            Type type = item.GetType();
            if (!hasEnablementViewModel.ContainsKey(type))
            {
                var doesTypeHasQuestionState = type.GetProperties().Any(ptp => ptp.Name == "QuestionState");
                hasEnablementViewModel[type] = doesTypeHasQuestionState;
            }

            if (hasEnablementViewModel[type])
            {
                var enablementModel = item.QuestionState.Enablement;
                return (EnablementViewModel)enablementModel;
            }

            return null;
        }

        public int GetItemLayoutId(int fromViewType)
        {
            return fromViewType;
        }
    }
}
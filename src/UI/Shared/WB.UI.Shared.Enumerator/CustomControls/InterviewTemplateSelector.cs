using System;
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
        private const int UnknownViewType = -1;

        private static readonly Dictionary<Type, int> EntityTemplates = new Dictionary<Type, int>
        {
            {typeof (StaticTextViewModel), Resource.Layout.interview_static_text},
            {typeof (TextListQuestionViewModel), Resource.Layout.interview_question_text_list},
            {typeof (TextListItemViewModel), Resource.Layout.interview_question_text_list_item},
            {typeof (TextListAddNewItemViewModel), Resource.Layout.interview_question_text_list_add_new_item},
            {typeof (TextQuestionViewModel), Resource.Layout.interview_question_text},
            {typeof (IntegerQuestionViewModel), Resource.Layout.interview_question_integer},
            {typeof (RealQuestionViewModel), Resource.Layout.interview_question_real},
            {typeof (GpsCoordinatesQuestionViewModel), Resource.Layout.interview_question_gps},
            {typeof (MultimediaQuestionViewModel), Resource.Layout.interview_question_multimedia},
            {typeof (SingleOptionQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (SingleOptionLinkedQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (SingleOptionLinkedToListQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (SingleOptionRosterLinkedQuestionViewModel), Resource.Layout.interview_question_single_option},
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (MultiOptionLinkedToRosterQuestionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (MultiOptionLinkedToListQuestionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (MultiOptionLinkedToRosterQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_question_datetime},
            {typeof (TimestampQuestionViewModel), Resource.Layout.interview_question_timestamp},
            {typeof (FilteredSingleOptionQuestionViewModel), Resource.Layout.interview_question_filtered_single_option },
            {typeof (CascadingSingleOptionQuestionViewModel), Resource.Layout.interview_question_filtered_single_option },
            {typeof (QRBarcodeQuestionViewModel), Resource.Layout.interview_question_qrbarcode},
            {typeof (YesNoQuestionViewModel), Resource.Layout.interview_question_yesno},
            {typeof (GroupViewModel), Resource.Layout.interview_group},
            {typeof (StartInterviewViewModel), Resource.Layout.prefilled_questions_start_button},
            {typeof (CompleteInterviewViewModel), Resource.Layout.interview_complete},
            {typeof (MultiOptionQuestionOptionViewModel), Resource.Layout.interview_question_multi_option_item},
            {typeof (MultiOptionLinkedQuestionOptionViewModel), Resource.Layout.interview_question_multi_option_item},
            {typeof (SingleOptionQuestionOptionViewModel), Resource.Layout.interview_question_single_option_item},
            {typeof (SingleOptionLinkedQuestionOptionViewModel), Resource.Layout.interview_question_single_option_item},
            {typeof (QuestionHeaderViewModel), Resource.Layout.interview_question__header},
            {typeof (ValidityViewModel), Resource.Layout.interview_question__validation},
            {typeof (WarningsViewModel), Resource.Layout.interview_question__warnings},
            {typeof (CommentsViewModel), Resource.Layout.interview_question__comments},
            {typeof (QuestionInstructionViewModel), Resource.Layout.interview_question__instructions},
            {typeof (AnsweringViewModel), Resource.Layout.interview_question__progressbar},
            {typeof (YesNoQuestionOptionViewModel), Resource.Layout.interview_question_yesno_item},
            {typeof (VariableViewModel), Resource.Layout.interview_variable},
            {typeof (ReadOnlyQuestionViewModel), Resource.Layout.interview_question_readonly},
            {typeof (AreaQuestionViewModel), Resource.Layout.interview_question_area},
            {typeof (AudioQuestionViewModel), Resource.Layout.interview_question_audio},
            {typeof (OptionBorderViewModel), Resource.Layout.interview_question_option_rounded_corner}
        };

        public int GetItemViewType(object forItemObject)
        {
            var typeOfViewModel = forItemObject.GetType();

            var disabledViewModelTypes = new[]
            {
                typeof(QuestionHeaderViewModel),
                typeof(GroupViewModel),
                typeof(StaticTextViewModel)
            };

            if (disabledViewModelTypes.Contains(typeOfViewModel))
            {
                var enablementModel = this.GetEnablementViewModel(forItemObject);

                if (enablementModel != null && !enablementModel.Enabled)
                {
                    if (enablementModel.HideIfDisabled) return UnknownViewType;

                    if (typeOfViewModel == typeof(QuestionHeaderViewModel))
                        return Resource.Layout.interview_disabled_question;

                    if (typeOfViewModel == typeof(GroupViewModel))
                        return Resource.Layout.interview_disabled_group;

                    if (typeOfViewModel == typeof(StaticTextViewModel))
                        return Resource.Layout.interview_disabled_static_text;
                }
            }

            if (forItemObject is GroupNavigationViewModel groupNavigationViewModel)
            {
                var groupViewModel = groupNavigationViewModel;
                if (groupViewModel.NavigationGroupType == NavigationGroupType.ToParentGroup)
                {
                    return Resource.Layout.interview_group_to_parent_navigation;
                }
                else
                {
                    return Resource.Layout.interview_group_navigation;
                }
            }

            if (forItemObject is MultimediaQuestionViewModel multimediaQuestionViewModel && multimediaQuestionViewModel.IsSignature)
            {
                return Resource.Layout.interview_question_signature;
            }

            return EntityTemplates.ContainsKey(typeOfViewModel)
                ? EntityTemplates[typeOfViewModel]
                : EntityTemplates.ContainsKey(typeOfViewModel.BaseType) ? EntityTemplates[typeOfViewModel.BaseType] : UnknownViewType;
        }

        private EnablementViewModel GetEnablementViewModel(object item)
        {
            switch (item)
            {
                case QuestionHeaderViewModel questionHeaderViewModel:
                    return questionHeaderViewModel.Enablement;
                case GroupViewModel groupViewModel:
                    return groupViewModel.Enablement;
                case StaticTextViewModel staticTextViewModel:
                    return staticTextViewModel.QuestionState.Enablement;
            }

            return null;
        }

        public int GetItemLayoutId(int fromViewType)
        {
            return fromViewType;
        }

        public int ItemTemplateId { get; set; }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Android.Content;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Support.RecyclerView;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using GroupViewModel = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups.GroupViewModel;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class InterviewEntityAdapter : MvxRecyclerAdapter
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
            {typeof (MultiOptionQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (MultiOptionLinkedQuestionViewModel), Resource.Layout.interview_question_multi_option},
            {typeof (DateTimeQuestionViewModel), Resource.Layout.interview_question_datetime},
            {typeof (FilteredSingleOptionQuestionViewModel), Resource.Layout.interview_question_filtered_single_option },
            {typeof (CascadingSingleOptionQuestionViewModel), Resource.Layout.interview_question_cascading_single_option },
            {typeof (QRBarcodeQuestionViewModel), Resource.Layout.interview_question_qrbarcode},
            {typeof (YesNoQuestionViewModel), Resource.Layout.interview_question_yesno},
            {typeof (GroupViewModel), Resource.Layout.interview_group},
            {typeof (GroupNavigationViewModel), Resource.Layout.interview_group_navigation},
            {typeof (StartInterviewViewModel), Resource.Layout.prefilled_questions_start_button},
            {typeof (CompleteInterviewViewModel), Resource.Layout.interview_complete_status_change},
        };


        public InterviewEntityAdapter(IMvxAndroidBindingContext bindingContext)
            : base(bindingContext)
        {
        }

        public override int GetItemViewType(int position)
        {
            object source = this.GetItem(position);

            var typeOfViewModel = source.GetType();

            if (typeOfViewModel.Name.EndsWith("QuestionViewModel"))
            {
                var enablementModel = this.GetEnablementViewModel(source);
                if (enablementModel != null && !enablementModel.Enabled)
                {
                    return Resource.Layout.interview_disabled_question;
                }
            }

            if (typeOfViewModel == typeof(GroupViewModel))
            {
                var enablementModel = this.GetEnablementViewModel(source);
                if (enablementModel != null && !enablementModel.Enabled)
                {
                    return Resource.Layout.interview_disabled_group;
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

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            
            return viewType != UnknownViewType
                ? bindingContext.BindingInflate(viewType, parent, false)
                : this.CreateEmptyView(parent.Context);
        }

        private View CreateEmptyView(Context context)
        {
            return new View(context);
        }

        protected override void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                base.NotifyDataSetChanged(e);
            }
            catch { }
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.UI.Shared.Enumerator.Activities;
using GroupViewModel = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups.GroupViewModel;
using Object = Java.Lang.Object;

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
            {typeof (SingleOptionRosterLinkedQuestionViewModel), Resource.Layout.interview_question_single_option},
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
            using (var handler = new Handler(Looper.MainLooper))
            {
                handler.Post(() => this.NotifyDataSetChanged(e));
            }
        }

        public override void OnViewDetachedFromWindow(Object holder)
        {
            // we do this, because same bindings use focus as triger, 
            // but in new version of MvvmCross focus event is raised after clear data in control
            bool isFocusedChildren = IsThereChildrenWithFocus(holder);
            if (isFocusedChildren) 
            {
                IMvxAndroidCurrentTopActivity topActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
                var activity = topActivity.Activity;
                activity.RemoveFocusFromEditText();
            }

            base.OnViewDetachedFromWindow(holder);
        }

        private bool IsThereChildrenWithFocus(Object holder)
        {
            var viewHolder = holder as RecyclerView.ViewHolder;
            if (viewHolder != null)
                return IsThereChildrenWithFocus(viewHolder.ItemView);

            var view = holder as View;

            if (view == null)
                return false;

            if (view.IsFocused)
                return true;

            var viewGroup = view as ViewGroup;
            if (viewGroup != null)
                return IsThereChildrenWithFocus(viewGroup.FocusedChild);

            return false;
        }
    }
}
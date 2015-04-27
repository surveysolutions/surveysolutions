using System.Collections.Generic;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Android.Content;
using System;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.QuestionnaireTester.Views.CustomControls;


namespace WB.UI.QuestionnaireTester.Views.Adapters
{
    public class InterviewEntityAdapter : MvxRecyclerViewAdapter
    {
        public InterviewEntityAdapter(Context context, IMvxAndroidBindingContext bindingContext)
            : base(context, bindingContext)
        {
        }
       
        private static readonly Dictionary<Type, int> GroupItemsTemplates = new Dictionary<Type, int>
        {
            {typeof (StaticTextViewModel), Resource.Layout.interview_static_text },
            {typeof (GroupReferenceViewModel), Resource.Layout.interview_group_reference },
            {typeof (RostersReferenceViewModel), Resource.Layout.interview_roster_reference },
            {typeof (QuestionContainerViewModel<>), Resource.Layout.interview_question_container }
        };

        public override int GetItemViewType(int position)
        {
            var viewModel = this.GetRawItem(position);
            var viewModelType = viewModel.GetType();

            if (viewModelType.IsSubclassOfRawGeneric(typeof(QuestionContainerViewModel<>)))
            {
                viewModelType = typeof(QuestionContainerViewModel<>);
            }

            return GroupItemsTemplates[viewModelType];
        }

        protected override View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return bindingContext.BindingInflate(viewType, parent, false);
        }
    }
}


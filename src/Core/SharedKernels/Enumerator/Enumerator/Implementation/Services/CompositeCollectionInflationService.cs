using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class CompositeCollectionInflationService : ICompositeCollectionInflationService
    {
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;

        public CompositeCollectionInflationService(IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.mainThreadDispatcher = mainThreadDispatcher;
        }

        public CompositeCollection<ICompositeEntity> GetInflatedCompositeCollection(List<IInterviewEntityViewModel> newGroupItems)
        {
            var allVisibleGroupItems = new CompositeCollection<ICompositeEntity>();

            foreach (var interviewEntityViewModel in newGroupItems)
            {
                var compositeItem = interviewEntityViewModel as ICompositeQuestion;
                var rosterViewModel = interviewEntityViewModel as RosterViewModel;
                if (compositeItem != null)
                {
                    allVisibleGroupItems.Add(compositeItem.QuestionState.Header);

                    var questionCompositeCollection = new CompositeCollection<ICompositeEntity>();

                    if (compositeItem.QuestionState.Enablement.Enabled)
                        AddCompositeItemsOfQuestionToCollection(compositeItem, questionCompositeCollection);

                    compositeItem.QuestionState.Enablement.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                        OnEnablementChanged(allVisibleGroupItems, questionCompositeCollection, compositeItem);
                    };

                    allVisibleGroupItems.AddCollection(questionCompositeCollection);

                    allVisibleGroupItems.Add(compositeItem.Answering);
                    allVisibleGroupItems.Add(new QuestionDivider());
                }
                else if (rosterViewModel != null)
                {
                    allVisibleGroupItems.AddCollection(rosterViewModel.RosterInstances);
                }
                else
                {
                    allVisibleGroupItems.Add(interviewEntityViewModel);
                }
            }

            return allVisibleGroupItems;
        }

        private static void AddCompositeItemsOfQuestionToCollection(ICompositeQuestion compositeItem, CompositeCollection<ICompositeEntity> collection)
        {
            if (compositeItem.InstructionViewModel.HasInstructions)
                collection.Add(compositeItem.InstructionViewModel);

            collection.Add(compositeItem);

            var compositeItemWithChildren = compositeItem as ICompositeQuestionWithChildren;
            if (compositeItemWithChildren != null)
                collection.AddCollection(compositeItemWithChildren.Children);

            collection.Add(compositeItem.QuestionState.Validity);

            collection.Add(compositeItem.QuestionState.Comments);
        }

        private void OnEnablementChanged(CompositeCollection<ICompositeEntity> allVisibleGroupItems, 
            CompositeCollection<ICompositeEntity> questionItemsCollection,
            ICompositeQuestion compositeItem)
        {
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                var isViewModelInCollection = questionItemsCollection.Contains(compositeItem);

                if (compositeItem.QuestionState.Enablement.Enabled && !isViewModelInCollection)
                    AddCompositeItemsOfQuestionToCollection(compositeItem, questionItemsCollection);

                if (!compositeItem.QuestionState.Enablement.Enabled && isViewModelInCollection)
                {
                    questionItemsCollection.Clear();
                    allVisibleGroupItems.NotifyItemChanged(compositeItem.QuestionState.Header);
                }
            });
        }
    }
}
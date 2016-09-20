using System.Collections.Generic;
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
                    var compositeQuestionParts = new Dictionary<CompositeItemType, CompositeCollection<ICompositeEntity>>();

                    compositeQuestionParts.Add(CompositeItemType.Title,  new CompositeCollection<ICompositeEntity>());

                    if (compositeItem.InstructionViewModel.HasInstructions)
                        compositeQuestionParts.Add(CompositeItemType.Instruction, new CompositeCollection<ICompositeEntity>());

                    compositeQuestionParts.Add(CompositeItemType.Self, new CompositeCollection<ICompositeEntity>());

                    var compositeItemWithChildren = compositeItem as ICompositeQuestionWithChildren;
                    if (compositeItemWithChildren != null)
                        compositeQuestionParts.Add(CompositeItemType.Childrens, new CompositeCollection<ICompositeEntity>());

                    compositeQuestionParts.Add(CompositeItemType.Validity, new CompositeCollection<ICompositeEntity>());
                    compositeQuestionParts.Add(CompositeItemType.Comments, new CompositeCollection<ICompositeEntity>());
                    compositeQuestionParts.Add(CompositeItemType.AnsweringProgress, new CompositeCollection<ICompositeEntity>());

                    if (compositeItem.QuestionState.Enablement.Enabled)
                        OnEnablementChanged(compositeQuestionParts, compositeItem);

                    compositeItem.QuestionState.Enablement.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                        OnEnablementChanged(compositeQuestionParts, compositeItem);
                    };

                    compositeQuestionParts.Values.ForEach(x => allVisibleGroupItems.AddCollection(x));
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

        private void OnEnablementChanged(Dictionary<CompositeItemType, CompositeCollection<ICompositeEntity>> itemCompositeCollections, ICompositeQuestion compositeQuestion)
        {
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                if (!compositeQuestion.QuestionState.Enablement.Enabled)
                {
                    foreach (var itemCompositeCollection in itemCompositeCollections)
                    {
                        if (itemCompositeCollection.Key == CompositeItemType.Title &&
                            !compositeQuestion.QuestionState.Enablement.HideIfDisabled) continue;

                        itemCompositeCollection.Value.Clear();
                    }
                }
                else
                {
                    itemCompositeCollections[CompositeItemType.Title].Add(compositeQuestion.QuestionState.Header);

                    if (itemCompositeCollections.ContainsKey(CompositeItemType.Instruction))
                    {
                        itemCompositeCollections[CompositeItemType.Instruction].Add(compositeQuestion.InstructionViewModel);
                    }
                        
                    itemCompositeCollections[CompositeItemType.Self].Add(compositeQuestion);

                    if (itemCompositeCollections.ContainsKey(CompositeItemType.Childrens))
                    {
                        var compositeItemWithChildren = compositeQuestion as ICompositeQuestionWithChildren;
                        if (compositeItemWithChildren != null)
                            itemCompositeCollections[CompositeItemType.Childrens].AddCollection(compositeItemWithChildren.Children);
                    }
                        
                    itemCompositeCollections[CompositeItemType.Validity].Add(compositeQuestion.QuestionState.Validity);
                    itemCompositeCollections[CompositeItemType.Comments].Add(compositeQuestion.QuestionState.Comments);
                    itemCompositeCollections[CompositeItemType.AnsweringProgress].Add(compositeQuestion.Answering);
                }

            });
        }

        private enum CompositeItemType
        {
            Self = 1,
            Childrens,
            Validity,
            Comments,
            Instruction,
            AnsweringProgress,
            Title
        }
    }
}
﻿using System.Collections.Generic;
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

                    compositeQuestionParts.Select(compositeItemPart => new { Type = compositeItemPart.Key, CompositeCollection = compositeItemPart.Value })
                        .OrderBy(x => x.Type)
                        .ForEach(x => allVisibleGroupItems.AddCollection(x.CompositeCollection));

                    this.OnEnablementChanged(compositeQuestionParts, compositeItem, allVisibleGroupItems);

                    compositeItem.QuestionState.Enablement.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                        OnEnablementChanged(compositeQuestionParts, compositeItem, allVisibleGroupItems);
                    };

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

        private void OnEnablementChanged(
            Dictionary<CompositeItemType, CompositeCollection<ICompositeEntity>> itemCompositeCollections,
            ICompositeQuestion compositeQuestion, CompositeCollection<ICompositeEntity> allVisibleGroupItems)
        {
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                if (!itemCompositeCollections[CompositeItemType.Title].Contains(compositeQuestion.QuestionState.Header))
                {
                    itemCompositeCollections[CompositeItemType.Title].Add(compositeQuestion.QuestionState.Header);
                }

                if (!compositeQuestion.QuestionState.Enablement.Enabled)
                {
                    foreach (var itemCompositeCollection in itemCompositeCollections)
                    {
                        if (itemCompositeCollection.Key == CompositeItemType.Title &&
                            !compositeQuestion.QuestionState.Enablement.HideIfDisabled)
                        {
                            allVisibleGroupItems.NotifyItemChanged(compositeQuestion.QuestionState.Header);
                            continue;
                        }

                        itemCompositeCollection.Value.Clear();
                    }
                }
                else
                {
                    if (itemCompositeCollections.ContainsKey(CompositeItemType.Instruction))
                    {
                        itemCompositeCollections[CompositeItemType.Instruction].Add(
                            compositeQuestion.InstructionViewModel);
                    }

                    itemCompositeCollections[CompositeItemType.Self].Add(compositeQuestion);

                    if (itemCompositeCollections.ContainsKey(CompositeItemType.Childrens))
                    {
                        var compositeItemWithChildren = compositeQuestion as ICompositeQuestionWithChildren;
                        if (compositeItemWithChildren != null)
                            itemCompositeCollections[CompositeItemType.Childrens].AddCollection(
                                compositeItemWithChildren.Children);
                    }

                    itemCompositeCollections[CompositeItemType.Validity].Add(compositeQuestion.QuestionState.Validity);
                    itemCompositeCollections[CompositeItemType.Comments].Add(compositeQuestion.QuestionState.Comments);
                    itemCompositeCollections[CompositeItemType.AnsweringProgress].Add(compositeQuestion.Answering);
                }

            });
        }

        private enum CompositeItemType
        {
            Title = 1,
            Instruction = 2,
            Self = 3,
            Childrens = 4,
            Validity = 5,
            Comments = 6,
            AnsweringProgress = 7
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class CompositeCollectionInflationService : ICompositeCollectionInflationService
    {
        public CompositeCollection<ICompositeEntity> GetInflatedCompositeCollection(List<IInterviewEntityViewModel> newGroupItems)
        {
            var collection = new CompositeCollection<ICompositeEntity>();

            foreach (var interviewEntityViewModel in newGroupItems)
            {
                var compositeItem = interviewEntityViewModel as ICompositeQuestion;
                var rosterViewModel = interviewEntityViewModel as RosterViewModel;
                if (compositeItem != null)
                {
                    collection.Add(compositeItem.QuestionState.Header);

                    if (compositeItem.QuestionState.Enablement.Enabled)
                        FillCollection(compositeItem, collection);

                    compositeItem.Answering.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(AnsweringViewModel.InProgress)) return;
                        OnIsInProgressChanged(collection, compositeItem);
                    };
                    compositeItem.QuestionState.Validity.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(ValidityViewModel.IsInvalid)) return;
                        OnIsValidChanged(collection, compositeItem);
                    };
                    compositeItem.QuestionState.Enablement.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                        OnEnablementChanged(collection, compositeItem);
                    };
                    
                    collection.Add(new QuestionDivider());
                }
                else if (rosterViewModel != null)
                {
                    collection.AddCollection(rosterViewModel.RosterInstances);
                }
                else
                {
                    collection.Add(interviewEntityViewModel);
                }
            }

            return collection;
        }

        private static void FillCollection(ICompositeQuestion compositeItem, CompositeCollection<ICompositeEntity> collection1)
        {
            var headerViewModelIndex = collection1.IndexOf(compositeItem.QuestionState.Header);

            var collection = new List<object>();

            if (compositeItem.InstructionViewModel.HasInstructions)
                collection.Add(compositeItem.InstructionViewModel);

            collection.Add(compositeItem);

            var compositeItemWithChildren = compositeItem as ICompositeQuestionWithChildren;
            if (compositeItemWithChildren != null)
                collection.Add(compositeItemWithChildren.Children);

            if (compositeItem.QuestionState.Validity.IsInvalid)
                collection.Add(compositeItem.QuestionState.Validity);

            collection.Add(compositeItem.QuestionState.Comments);

            if (compositeItem.Answering.InProgress)
                collection.Add(compositeItem.Answering);

            collection1.Insert(headerViewModelIndex + 1, collection);
        }

        private static void RemoveFromCollection(ICompositeQuestion compositeItem, CompositeCollection<ICompositeEntity> collection)
        {
            var viewModelsToRemove = new List<ICompositeEntity>
            {
                compositeItem.InstructionViewModel,
                compositeItem,
                compositeItem.QuestionState.Validity,
                compositeItem.QuestionState.Comments,
                compositeItem.Answering
            };
            
            var compositeItemWithChildren = compositeItem as ICompositeQuestionWithChildren;
            if (compositeItemWithChildren != null)
                viewModelsToRemove.AddRange(compositeItemWithChildren.Children);

            collection.Remove(viewModelsToRemove);
        }

        private static void OnIsInProgressChanged(CompositeCollection<ICompositeEntity> collection, ICompositeQuestion compositeItem)
        {
            if (!compositeItem.QuestionState.Enablement.Enabled) return;

            var isViewModelInCollection = collection.Contains(compositeItem.Answering);

            if (compositeItem.Answering.InProgress && !isViewModelInCollection)
                collection.Insert(collection.IndexOf(compositeItem.QuestionState.Comments) + 1, compositeItem.Answering);

            if (!compositeItem.Answering.InProgress && isViewModelInCollection)
                collection.Remove(compositeItem.Answering);
        }

        private static void OnEnablementChanged(CompositeCollection<ICompositeEntity> collection, ICompositeQuestion compositeItem)
        {
            var isViewModelInCollection = collection.Contains(compositeItem);

            if (compositeItem.QuestionState.Enablement.Enabled && !isViewModelInCollection)
                FillCollection(compositeItem, collection);

            if (!compositeItem.QuestionState.Enablement.Enabled && isViewModelInCollection)
                RemoveFromCollection(compositeItem, collection);
        }

        private static void OnIsValidChanged(CompositeCollection<ICompositeEntity> collection, ICompositeQuestion compositeItem)
        {
            if (!compositeItem.QuestionState.Enablement.Enabled) return;

            var isViewModelInCollection = collection.Contains(compositeItem.QuestionState.Validity);

            if (compositeItem.QuestionState.Validity.IsInvalid && !isViewModelInCollection)
                collection.Insert(collection.IndexOf(compositeItem.QuestionState.Comments), compositeItem.QuestionState.Validity);

            if (!compositeItem.QuestionState.Validity.IsInvalid && isViewModelInCollection)
                collection.Remove(compositeItem.QuestionState.Validity);
        }
    }
}
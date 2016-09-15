using System.Collections.Generic;
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
        public CompositeCollection<ICompositeEntity> GetInflatedCompositeCollection(List<IInterviewEntityViewModel> newGroupItems)
        {
            var compositeCollection = new CompositeCollection<ICompositeEntity>();

            foreach (var interviewEntityViewModel in newGroupItems)
            {
                var compositeItem = interviewEntityViewModel as ICompositeQuestion;
                var rosterViewModel = interviewEntityViewModel as RosterViewModel;
                if (compositeItem != null)
                {
                    compositeCollection.Add(compositeItem.QuestionState.Header);

                    var questionCompositeCollection = new CompositeCollection<ICompositeEntity>();

                    if (compositeItem.QuestionState.Enablement.Enabled)
                        AddCompositeItemsOfQuestionToCollection(compositeItem, questionCompositeCollection);

                    compositeItem.QuestionState.Validity.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(ValidityViewModel.IsInvalid)) return;
                        OnIsValidChanged(questionCompositeCollection, compositeItem);
                    };
                    compositeItem.QuestionState.Enablement.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                        OnEnablementChanged(questionCompositeCollection, compositeItem);
                    };
                    
                    compositeCollection.AddCollection(questionCompositeCollection);

                    compositeCollection.Add(compositeItem.Answering);
                    compositeCollection.Add(new QuestionDivider());
                }
                else if (rosterViewModel != null)
                {
                    compositeCollection.AddCollection(rosterViewModel.RosterInstances);
                }
                else
                {
                    compositeCollection.Add(interviewEntityViewModel);
                }
            }

            return compositeCollection;
        }

        private static void AddCompositeItemsOfQuestionToCollection(ICompositeQuestion compositeItem, CompositeCollection<ICompositeEntity> collection)
        {
            if (compositeItem.InstructionViewModel.HasInstructions)
                collection.Add(compositeItem.InstructionViewModel);

            collection.Add(compositeItem);

            var compositeItemWithChildren = compositeItem as ICompositeQuestionWithChildren;
            if (compositeItemWithChildren != null)
                collection.AddCollection(compositeItemWithChildren.Children);

            if (compositeItem.QuestionState.Validity.IsInvalid)
                collection.Add(compositeItem.QuestionState.Validity);

            collection.Add(compositeItem.QuestionState.Comments);

            if (compositeItem.Answering.InProgress)
                collection.Add(compositeItem.Answering);
        }

        private static void OnEnablementChanged(CompositeCollection<ICompositeEntity> collection, ICompositeQuestion compositeItem)
        {
            var isViewModelInCollection = collection.Contains(compositeItem);

            if (compositeItem.QuestionState.Enablement.Enabled && !isViewModelInCollection)
                AddCompositeItemsOfQuestionToCollection(compositeItem, collection);

            if (!compositeItem.QuestionState.Enablement.Enabled && isViewModelInCollection)
                collection.Clear();
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
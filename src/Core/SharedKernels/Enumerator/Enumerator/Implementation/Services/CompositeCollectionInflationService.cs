using System;
using System.Collections.Generic;
using System.ComponentModel;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

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
                    if (compositeItem.InstructionViewModel.HasInstructions)
                        collection.Add(compositeItem.InstructionViewModel);

                    collection.Add(compositeItem);

                    var compositeItemWithChildren = compositeItem as ICompositeQuestionWithChildren;
                    if (compositeItemWithChildren != null)
                    {
                        collection.AddCollection(compositeItemWithChildren.Children);
                    }

                    collection.AddCollection(CreateViewModelAsCompositeCollectionRefreshedByChangesInField(
                        compositeItem.QuestionState.Validity,
                        nameof(compositeItem.QuestionState.Validity.IsInvalid),
                        () => compositeItem.QuestionState.Validity.IsInvalid));
                    collection.Add(compositeItem.QuestionState.Comments);
                    collection.AddCollection(CreateViewModelAsCompositeCollectionRefreshedByChangesInField(
                        compositeItem.Answering,
                        nameof(compositeItem.Answering.InProgress),
                        () => compositeItem.Answering.InProgress));
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

        private static CompositeCollection<ICompositeEntity> CreateViewModelAsCompositeCollectionRefreshedByChangesInField(
            ICompositeEntity viewModel,
            string propertyNameToRefresh,
            Func<bool> doesNeedShowViewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            var notifyPropertyChanged = viewModel as INotifyPropertyChanged;
            if (notifyPropertyChanged == null)
                throw new ArgumentException("ViewModel should support INotifyPropertyChanged interface. ViewModel: " + viewModel.GetType().Name);

            CompositeCollection<ICompositeEntity> collection = new CompositeCollection<ICompositeEntity>();
            notifyPropertyChanged.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == propertyNameToRefresh)
                {
                    bool isNeedShow = doesNeedShowViewModel.Invoke();
                    var isShowing = collection.Contains((ICompositeEntity)viewModel);

                    if (isNeedShow && !isShowing)
                    {
                        collection.Add(viewModel);
                    }
                    else if (!isNeedShow && isShowing)
                    {
                        collection.Clear();
                    }

                }
            };
            return collection;
        }
    }
}

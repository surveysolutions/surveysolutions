using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
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
        private readonly IMvxMainThreadAsyncDispatcher mainThreadDispatcher;

        public CompositeCollectionInflationService(IMvxMainThreadAsyncDispatcher mainThreadDispatcher)
        {
            this.mainThreadDispatcher = mainThreadDispatcher;
        }

        public CompositeCollection<ICompositeEntity> GetInflatedCompositeCollection(List<IInterviewEntityViewModel> newGroupItems)
        {
            var allVisibleGroupItems = new CompositeCollection<ICompositeEntity>();

            foreach (var interviewEntityViewModel in newGroupItems)
            {
                var compositeQuestion = interviewEntityViewModel as ICompositeQuestion;
                var rosterViewModel = interviewEntityViewModel as RosterViewModel;
                if (compositeQuestion != null)
                {
                    var compositeQuestionParts = new Dictionary<CompositeItemType, CompositeCollection<ICompositeEntity>>();

                    compositeQuestionParts.Add(CompositeItemType.Title,  new CompositeCollection<ICompositeEntity>());

                    if (compositeQuestion.InstructionViewModel.HasInstructions)
                        compositeQuestionParts.Add(CompositeItemType.Instruction, new CompositeCollection<ICompositeEntity>());

                    compositeQuestionParts.Add(CompositeItemType.Self, new CompositeCollection<ICompositeEntity>());

                    if (compositeQuestion is ICompositeQuestionWithChildren compositeItemWithChildren)
                        compositeQuestionParts.Add(CompositeItemType.Children, new CompositeCollection<ICompositeEntity>());

                    compositeQuestionParts.Add(CompositeItemType.Validity, new CompositeCollection<ICompositeEntity>());
                    compositeQuestionParts.Add(CompositeItemType.Warnings, new CompositeCollection<ICompositeEntity>());
                    compositeQuestionParts.Add(CompositeItemType.Comments, new CompositeCollection<ICompositeEntity>());
                    compositeQuestionParts.Add(CompositeItemType.AnsweringProgress, new CompositeCollection<ICompositeEntity>());

                    compositeQuestionParts.Select(compositeItemPart => new { Type = compositeItemPart.Key, CompositeCollection = compositeItemPart.Value })
                        .OrderBy(x => x.Type)
                        .ForEach(x => allVisibleGroupItems.AddCollection(x.CompositeCollection));

                    this.OnEnablementChanged(compositeQuestionParts, compositeQuestion, allVisibleGroupItems);

                    compositeQuestion.QuestionState.Enablement.PropertyChanged += async (sender, e) =>
                    {
                        if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                        await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
                        {
                            OnEnablementChanged(compositeQuestionParts, compositeQuestion, allVisibleGroupItems);
                        });
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
            ICompositeQuestion compositeQuestion,
            CompositeCollection<ICompositeEntity> allVisibleGroupItems)
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

                if (itemCompositeCollections.ContainsKey(CompositeItemType.Children))
                {
                    if (compositeQuestion is ICompositeQuestionWithChildren compositeItemWithChildren)
                        itemCompositeCollections[CompositeItemType.Children].AddCollection(
                            compositeItemWithChildren.Children);
                }

                itemCompositeCollections[CompositeItemType.Validity].Add(compositeQuestion.QuestionState.Validity);
                itemCompositeCollections[CompositeItemType.Warnings].Add(compositeQuestion.QuestionState.Warnings);
                itemCompositeCollections[CompositeItemType.Comments].Add(compositeQuestion.QuestionState.Comments);
                itemCompositeCollections[CompositeItemType.AnsweringProgress].Add(compositeQuestion.Answering);
            }
        }

        private enum CompositeItemType
        {
            Title = 1,
            Instruction = 2,
            Self = 3,
            Children = 4,
            Validity = 5,
            Warnings = 6,
            Comments = 7,
            AnsweringProgress = 8
        }
    }
}

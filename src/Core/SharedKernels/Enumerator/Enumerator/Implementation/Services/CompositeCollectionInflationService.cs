using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
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
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public CompositeCollectionInflationService(IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireRepository)
        {
            this.interviewRepository = interviewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        public CompositeCollection<ICompositeEntity> GetInflatedCompositeCollection( 
            string interviewId,
            IEnumerable<IInterviewEntityViewModel> newGroupItems)
        {
            var interview = interviewRepository.GetOrThrow(interviewId);
            var questionnaire = questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, null);

            var allVisibleGroupItems = new CompositeCollection<ICompositeEntity>();

            foreach (var interviewEntityViewModel in newGroupItems)
            {
                var lateInitViewModel = interviewEntityViewModel as IInterviewEntityLateInitViewModel;

                var entity = lateInitViewModel != null 
                    ? lateInitViewModel.WrappedEntity
                    : interviewEntityViewModel;

                if (entity is ICompositeQuestion compositeQuestion)
                {
                    if (lateInitViewModel != null && 
                        (interview.IsEnabled(lateInitViewModel.Identity)
                        || !questionnaire.ShouldBeHiddenIfDisabled(lateInitViewModel.Identity.Id)))
                        lateInitViewModel?.InitIfNeed();
                    // if (compositeQuestion is ICompositeQuestionWithChildren)
                    //     lateInitViewModel?.InitIfNeed();
                    InflateOneQuestion(lateInitViewModel, compositeQuestion, allVisibleGroupItems);
                }
                else if (entity is RosterViewModel rosterViewModel)
                {
                    lateInitViewModel?.InitIfNeed();
                    allVisibleGroupItems.AddCollection(rosterViewModel.RosterInstances);
                }
                else if (entity is FlatRosterViewModel flatRosterViewModel)
                {
                    lateInitViewModel?.InitIfNeed();
                    allVisibleGroupItems.AddCollection(flatRosterViewModel.RosterInstances);
                }
                else if (entity is StaticTextViewModel staticText)
                {
                    if (lateInitViewModel != null && 
                        (interview.IsEnabled(lateInitViewModel.Identity)
                         || !questionnaire.ShouldBeHiddenIfDisabled(lateInitViewModel.Identity.Id)))
                        lateInitViewModel?.InitIfNeed();
                    allVisibleGroupItems.Add(staticText);
                    staticText.StaticTextState.Enablement.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                        if (staticText.StaticTextState.Enablement.Enabled && staticText.StaticTextState.Enablement.HideIfDisabled)
                            lateInitViewModel?.InitIfNeed();
                        allVisibleGroupItems.NotifyItemChanged(staticText);
                    };
                }
                else
                {
                    lateInitViewModel?.InitIfNeed();
                    allVisibleGroupItems.Add(interviewEntityViewModel);
                }
            }

            return allVisibleGroupItems;
        }

        private void InflateOneQuestion(IInterviewEntityLateInitViewModel lateInitViewModel, ICompositeQuestion compositeQuestion, CompositeCollection<ICompositeEntity> allVisibleGroupItems)
        {
            var compositeQuestionParts = new Dictionary<CompositeItemType, CompositeCollection<ICompositeEntity>>();

            compositeQuestionParts.Add(CompositeItemType.Title, new CompositeCollection<ICompositeEntity>());

            if (compositeQuestion.InstructionViewModel.HasInstructions)
                compositeQuestionParts.Add(CompositeItemType.Instruction, new CompositeCollection<ICompositeEntity>());

            compositeQuestionParts.Add(CompositeItemType.Self, new CompositeCollection<ICompositeEntity>());

            if (compositeQuestion is ICompositeQuestionWithChildren)
                compositeQuestionParts.Add(CompositeItemType.Children, new CompositeCollection<ICompositeEntity>());

            compositeQuestionParts.Add(CompositeItemType.Validity, new CompositeCollection<ICompositeEntity>());
            compositeQuestionParts.Add(CompositeItemType.Warnings, new CompositeCollection<ICompositeEntity>());
            compositeQuestionParts.Add(CompositeItemType.Comments, new CompositeCollection<ICompositeEntity>());
            compositeQuestionParts.Add(CompositeItemType.AnsweringProgress, new CompositeCollection<ICompositeEntity>());

            compositeQuestionParts.Select(compositeItemPart =>
                    new {Type = compositeItemPart.Key, CompositeCollection = compositeItemPart.Value})
                .OrderBy(x => x.Type)
                .ForEach(x => allVisibleGroupItems.AddCollection(x.CompositeCollection));

            this.OnEnablementChanged(compositeQuestionParts, compositeQuestion, allVisibleGroupItems);

            compositeQuestion.QuestionState.Enablement.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != nameof(EnablementViewModel.Enabled)) return;
                if (compositeQuestion.QuestionState.Enablement.Enabled && compositeQuestion.QuestionState.Enablement.HideIfDisabled)
                    lateInitViewModel?.InitIfNeed();
                    
                OnEnablementChanged(compositeQuestionParts, compositeQuestion, allVisibleGroupItems);
            };
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

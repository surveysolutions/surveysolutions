using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedToRosterQuestionQuestionViewModel : MultiOptionLinkedQuestionBaseViewModel,
        ILiteEventHandler<LinkedOptionsChanged>,
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private Guid linkedToQuestionId;
        private HashSet<Guid> parentRosterIds;

        public MultiOptionLinkedToRosterQuestionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IPrincipal userIdentity, ILiteEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadDispatcher,
            ThrottlingViewModel throttlingModel)
            : base(questionState, answering, instructionViewModel, interviewRepository, questionnaireStorage, userIdentity, eventRegistry,
                mainThreadDispatcher, throttlingModel)
        {
        }

        protected override void InitFromModel(IQuestionnaire questionnaire)
        {
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(questionIdentity.Id);
            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id);
            this.parentRosterIds = questionnaire.GetRostersFromTopToSpecifiedEntity(this.linkedToQuestionId).ToHashSet();
        }

        protected override IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            var linkedQuestion = interview.GetLinkedMultiOptionQuestion(this.questionIdentity);

            var answeredOptions = linkedQuestion.GetAnswer()?.ToRosterVectorArray()?.Select(x => new RosterVector(x))?.ToArray() ??
                                  new RosterVector[0];

            foreach (var linkedOption in linkedQuestion.Options)
                yield return this.CreateOptionViewModel(linkedOption, answeredOptions, interview);

            base.UpdateMaxAnswersCountMessage(answeredOptions.Length);
        }

        public async void Handle(LinkedOptionsChanged @event)
        {
            var newOptions = this.CreateOptions();

            await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.Options.SynchronizeWith(newOptions.ToList(), (s, t) => s.Value.Identical(t.Value));
                this.RaisePropertyChanged(() => HasOptions);
            });
        }

        public async void Handle(RosterInstancesTitleChanged @event)
        {
            var optionListShouldBeUpdated = @event.ChangedInstances.Any(x => this.parentRosterIds.Contains(x.RosterInstance.GroupId));
            if (optionListShouldBeUpdated)
            {
                var newOptions = this.CreateOptions();
                await this.mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
                {
                    this.Options.SynchronizeWith(newOptions.ToList(), (s, t) => s.Value.Identical(t.Value) && s.Title == t.Title);
                    this.RaisePropertyChanged(() => HasOptions);
                });
            }
        }

        private MultiOptionLinkedQuestionOptionViewModel CreateOptionViewModel(RosterVector linkedOption, RosterVector[] answeredOptions, 
            IStatefulInterview interview)
            => new MultiOptionLinkedQuestionOptionViewModel(this)
            {
                Title = interview.GetLinkedOptionTitle(this.questionIdentity, linkedOption),
                Value = linkedOption,
                Checked = answeredOptions.Contains(linkedOption),
                QuestionState = this.questionState,
                CheckedOrder = this.areAnswersOrdered ? Array.FindIndex(answeredOptions, x => x.Identical(linkedOption)) + 1 : (int?)null,
                CanBeChecked = answeredOptions.Contains(linkedOption) || !this.maxAllowedAnswers.HasValue || answeredOptions.Length < this.maxAllowedAnswers
            };
    }
}

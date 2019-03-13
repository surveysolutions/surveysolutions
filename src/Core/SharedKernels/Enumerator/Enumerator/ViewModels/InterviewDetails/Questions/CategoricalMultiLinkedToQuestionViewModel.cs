using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CategoricalMultiLinkedToQuestionViewModel : CategoricalMultiViewModelBase<RosterVector, RosterVector>,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>,
        ILiteEventHandler<LinkedOptionsChanged>,
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        private RosterVector[] selectedOptionsToSave;
        private HashSet<Guid> parentRosters;

        public CategoricalMultiLinkedToQuestionViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository, ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository, IPrincipal principal, AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel, ThrottlingViewModel throttlingModel)
            : base(questionStateViewModel, questionnaireRepository, eventRegistry,
                interviewRepository, principal, answering, instructionViewModel, throttlingModel)
        {
            this.Options = new CovariantObservableCollection<CategoricalMultiOptionViewModel<RosterVector>>();
        }

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            var linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);
            this.parentRosters = questionnaire.GetRostersFromTopToSpecifiedEntity(linkedToQuestionId).ToHashSet();
        }

        protected override bool IsInterviewAnswer(RosterVector interviewAnswer, RosterVector optionValue)
            => interviewAnswer == optionValue;

        protected override void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<RosterVector>> answeredViewModels) 
            => this.selectedOptionsToSave = answeredViewModels.Select(x => x.Value).ToArray();
        
        protected override RosterVector[] GetAnsweredOptionsFromInterview(IStatefulInterview interview)
            => interview.GetLinkedMultiOptionQuestion(this.Identity)?.GetAnswer()?.ToRosterVectorArray();

        protected override void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<RosterVector> optionViewModel, RosterVector[] answers)
            => optionViewModel.Checked = answers.Contains(optionViewModel.Value);

        protected override AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId)
        => new AnswerMultipleOptionsLinkedQuestionCommand(
                interviewId,
                userId,
                this.Identity.Id,
                this.Identity.RosterVector,
                this.selectedOptionsToSave);

        protected override IEnumerable<CategoricalMultiOptionViewModel<RosterVector>> GetOptions(IStatefulInterview interview)
        {
            foreach (var answeredOption in interview.GetLinkedMultiOptionQuestion(this.Identity).Options)
            {
                var vm = new CategoricalMultiOptionViewModel<RosterVector>();
                base.InitViewModel(interview.GetLinkedOptionTitle(this.Identity, answeredOption), answeredOption, interview, vm);

                yield return vm;
            }
        }

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id || !@event.RosterVector.Identical(this.Identity.RosterVector)) return;
            this.UpdateViewModelsByAnsweredOptionsInMainThread(@event.SelectedRosterVectors?.Select(RosterVector.Convert)?.ToArray());
        }

        public void Handle(LinkedOptionsChanged @event)
        {
            if (@event.ChangedLinkedQuestions.All(x => x.QuestionId != this.Identity)) return;

            this.UpdateViewModelsInMainThread();
        }

        public virtual void Handle(RosterInstancesTitleChanged @event)
        {
            if (!@event.ChangedInstances.Any(x => this.parentRosters.Contains(x.RosterInstance.GroupId))) return;

            this.UpdateViewModelsInMainThread();
        }
    }
}

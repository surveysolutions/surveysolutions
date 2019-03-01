﻿using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
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
    public class CategoricalMultiLinkedToListViewModel : 
        CategoricalMultiViewModelBase<int, int>,
        ILiteEventHandler<TextListQuestionAnswered>,
        ILiteEventHandler<LinkedToListOptionsChanged>,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>
    {
        private int[] selectedOptionsToSave;
        private Guid linkedToQuestionId;

        public CategoricalMultiLinkedToListViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewModel,
            IQuestionnaireStorage questionnaireRepository, ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository, IPrincipal principal, AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel, ThrottlingViewModel throttlingModel) : base(
            questionStateViewModel, questionnaireRepository, eventRegistry, interviewRepository, principal, answering,
            instructionViewModel, throttlingModel)
        {
            this.Options = new CovariantObservableCollection<CategoricalMultiOptionViewModel<int>>();
        }

        protected override void Init(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(this.Identity.Id);
        }

        protected override void SaveAnsweredOptionsForThrottling(IOrderedEnumerable<CategoricalMultiOptionViewModel<int>> answeredViewModels) 
            => this.selectedOptionsToSave = answeredViewModels.Select(x => x.Value).ToArray();

        protected override int[] GetAnsweredOptionsFromInterview(IStatefulInterview interview) 
            => interview.GetMultiOptionLinkedToListQuestion(this.Identity)?.GetAnswer()?.CheckedValues.ToArray();

        protected override void SetAnswerToOptionViewModel(CategoricalMultiOptionViewModel<int> optionViewModel, int[] answers)
            => optionViewModel.Checked = answers.Contains(optionViewModel.Value);

        protected override AnswerQuestionCommand GetAnswerCommand(Guid interviewId, Guid userId)
            => new AnswerMultipleOptionsQuestionCommand(interviewId, userId, this.Identity.Id, this.Identity.RosterVector, this.selectedOptionsToSave);

        protected override IEnumerable<CategoricalMultiOptionViewModel<int>> GetOptions(IStatefulInterview interview)
        {
            var listQuestion = interview.FindQuestionInQuestionBranch(this.linkedToQuestionId, this.Identity);

            if (listQuestion == null || listQuestion.IsDisabled() || listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows == null)
                yield break;
            
            foreach (var textListAnswerRow in listQuestion.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows)
            {
                var vm = new CategoricalMultiOptionViewModel<int>();
                base.InitViewModel(textListAnswerRow.Text, textListAnswerRow.Value, interview, vm);

                yield return vm;
            }
        }

        protected override bool IsInterviewAnswer(int interviewAnswer, int optionValue)
            => interviewAnswer == optionValue;

        public void Handle(TextListQuestionAnswered @event)
        {
            if (@event.QuestionId != this.linkedToQuestionId)
                return;

            this.InvokeOnMainThread(() =>
            {
                foreach (var answer in @event.Answers)
                {
                    var option = this.Options.FirstOrDefault(o => o.Value == answer.Item1);
                    if (option != null) option.Title = answer.Item2;
                }
            });
        }

        public void Handle(LinkedToListOptionsChanged @event)
        {
            if (@event.ChangedLinkedQuestions.All(x => x.QuestionId != this.Identity)) return;

            this.InvokeOnMainThread(this.UpdateViewModels);
        }

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if (@event.QuestionId != this.Identity.Id || !@event.RosterVector.Identical(this.Identity.RosterVector)) return;

            this.InvokeOnMainThread(() => this.UpdateViewModelsByAnsweredOptions(@event.SelectedValues.Select(Convert.ToInt32).ToArray()));
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId)) return;

            this.InvokeOnMainThread(this.UpdateViewModels);
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId))
                return;

            this.InvokeOnMainThread(this.UpdateViewModels);
        }

        public override void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Contains(this.Identity))
                this.InvokeOnMainThread(()=>this.UpdateViewModelsByAnsweredOptions(Array.Empty<int>()));

            if (@event.Questions.Any(question => question.Id == this.linkedToQuestionId))
                this.InvokeOnMainThread(this.UpdateViewModels);
        }
    }
}

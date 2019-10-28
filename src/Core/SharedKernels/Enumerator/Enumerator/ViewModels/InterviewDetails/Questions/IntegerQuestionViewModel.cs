﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class IntegerQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IAsyncViewModelEventHandler<AnswersRemoved>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly IPrincipal principal;
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionService userInteractionService;
        private readonly SpecialValuesViewModel specialValues;
        private Identity questionIdentity;
        private string interviewId;

        public IQuestionStateViewModel QuestionState => this.questionState;
        public SpecialValuesViewModel SpecialValues => this.specialValues;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public AnsweringViewModel Answering { get; private set; }

        private readonly ThrottlingViewModel throttlingModel;

        private bool isRosterSizeQuestion;
        private int answerMaxValue;

        private decimal? previousAnswer;

        private decimal? answer;
        public decimal? Answer
        {
            get => this.answer;
            set
            {
                if (this.answer == value) return;
                this.answer = value;
                this.RaisePropertyChanged();
            }
        }

        public bool UseFormatting { get; set; }

        public IMvxAsyncCommand ValueChangeCommand => new MvxAsyncCommand(this.SendAnswerIntegerQuestionCommandAsync, () => this.principal.IsAuthenticated);

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswer);

        private readonly QuestionStateViewModel<NumericIntegerQuestionAnswered> questionState;
        private int? protectedAnswer;

        private async Task RemoveAnswer()
        {
            try
            {
                if (isRosterSizeQuestion)
                {
                    var amountOfRostersToRemove = this.previousAnswer;
                    if (this.previousAnswer > 0)
                    {
                        var message = string.Format(UIResources.Interview_Question_NumberRosterRemoveConfirm, amountOfRostersToRemove);
                        if (!(await this.userInteractionService.ConfirmAsync(message)))
                        {
                            this.Answer = this.previousAnswer;
                            await this.SpecialValues.SetAnswerAsync(this.previousAnswer);
                            return;
                        }
                    }
                }

                var command = new RemoveAnswerCommand(Guid.Parse(this.interviewId),
                    this.principal.CurrentUserIdentity.UserId,
                    this.questionIdentity);

                await this.Answering.SendRemoveAnswerCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public IntegerQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericIntegerQuestionAnswered> questionStateViewModel,
            IUserInteractionService userInteractionService,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IViewModelEventRegistry liteEventRegistry,
            SpecialValuesViewModel specialValues, 
            ThrottlingViewModel throttlingModel)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.questionState = questionStateViewModel;
            this.userInteractionService = userInteractionService;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.liteEventRegistry = liteEventRegistry;
            this.specialValues = specialValues;
            this.throttlingModel = throttlingModel;
            this.throttlingModel.Init(AnswerQuestion);
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionIdentity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            
            this.questionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);

            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.UseFormatting = questionnaire.ShouldUseFormatting(entityIdentity.Id);

            var answerModel = interview.GetIntegerQuestion(entityIdentity);
            if (answerModel.IsAnswered())
            {
                var answerValue = answerModel.GetAnswer().Value;
                this.Answer = answerValue;
                this.previousAnswer = this.Answer;
            }

            this.isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(entityIdentity.Id);
            var isRosterSizeOfLongRoster = questionnaire.IsQuestionIsRosterSizeForLongRoster(entityIdentity.Id);

            this.answerMaxValue = isRosterSizeOfLongRoster ? Constants.MaxLongRosterRowCount : Constants.MaxRosterRowCount;
            this.ProtectedAnswer = answerModel.ProtectedAnswer?.Value;

            InitSpecialValues(interviewId, entityIdentity);

            if (!this.specialValues.HasSpecialValues)
                this.throttlingModel.ThrottlePeriod = 0;

            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        public int? ProtectedAnswer
        {
            get => protectedAnswer;
            set => SetProperty(ref protectedAnswer, value);
        }

        private void InitSpecialValues(string interviewId, Identity entityIdentity)
        {
            specialValues.Init(interviewId, entityIdentity, this.questionState);
            this.specialValues.SpecialValueChanged += SpecialValueChanged;
            this.specialValues.SpecialValueRemoved += SpecialValueRemoved;

            if (specialValues.SpecialValues.Any(x => x.Selected))
            {
                this.Answer = null;
            }
        }

        private async void SpecialValueChanged(object sender, EventArgs eventArgs)
        {
            var selectedSpecialValue = (SingleOptionQuestionOptionViewModel)sender;
            await EnqueueSaveAnswer(selectedSpecialValue.Value, true);
        }

        private async void SpecialValueRemoved(object sender, EventArgs eventArgs)
        {
            await RemoveAnswer();
        }

        private async Task SendAnswerIntegerQuestionCommandAsync()
        {
            if (this.Answer == null)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                    .Interview_Question_Integer_EmptyValueError);
                return;
            }

            if (this.Answer > int.MaxValue || this.Answer < int.MinValue)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                    .Interview_Question_Integer_ParsingError);
                return;
            }

            await EnqueueSaveAnswer(this.Answer, specialValues.IsSpecialValueSelected(this.Answer));
        }

        
        private decimal? answerOrSelectedValueToSave = null;
        private bool isSpecialValueToSave = false;

        private async Task EnqueueSaveAnswer(decimal? answeredOrSelectedValue, bool isSpecialValueSelected)
        {
            this.answerOrSelectedValueToSave = answeredOrSelectedValue;
            this.isSpecialValueToSave = isSpecialValueSelected;

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private async Task AnswerQuestion()
        {
            var answeredOrSelectedValue = this.answerOrSelectedValueToSave;
            var isSpecialValueSelected = this.isSpecialValueToSave;

            if (ProtectedAnswer.HasValue && answeredOrSelectedValue < protectedAnswer)
            {
                var message = string.Format(UIResources.Interview_Questions_Integer_ProtectedValue,
                    ProtectedAnswer);
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                return;
            }

            if (this.isRosterSizeQuestion)
            {
                if (!isSpecialValueSelected && answeredOrSelectedValue < 0)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_NegativeRosterSizeAnswer,
                        answeredOrSelectedValue);
                    await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (answeredOrSelectedValue > this.answerMaxValue)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_RosterSizeAnswerMoreThanMaxValue,
                        answeredOrSelectedValue, this.answerMaxValue);
                    await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (this.previousAnswer.HasValue && this.previousAnswer > 0)
                {
                    var amountOfRostersToRemove = this.previousAnswer - Math.Max(answeredOrSelectedValue.Value, 0);
                    if (amountOfRostersToRemove > 0)
                    {
                        var message = string.Format(UIResources.Interview_Question_NumberRosterRemoveConfirm,
                            amountOfRostersToRemove);
                        if (!await this.userInteractionService.ConfirmAsync(message))
                        {
                            this.Answer = this.previousAnswer;
                            await this.specialValues.SetAnswerAsync(this.previousAnswer);
                            return;
                        }
                    }
                }
            }

            var command = new AnswerNumericIntegerQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answer: (int) answeredOrSelectedValue);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                this.previousAnswer = answeredOrSelectedValue;

                if (isSpecialValueSelected)
                {
                    this.Answer = null;
                }

                await this.specialValues.SetAnswerAsync(answeredOrSelectedValue);
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.liteEventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();

            this.specialValues.SpecialValueChanged -= SpecialValueChanged;
            this.specialValues.SpecialValueRemoved -= SpecialValueRemoved;
            this.specialValues.Dispose();

            this.throttlingModel.Dispose();
        }

        public async Task HandleAsync(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.Answer = null;
                    this.previousAnswer = null;
                    await this.specialValues.ClearSelectionAndShowValues();
                }
            }
        }

        public IObservableCollection<ICompositeEntity> Children => specialValues.AsChildren;
    }
}

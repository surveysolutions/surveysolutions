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
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class RealQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        IAsyncViewModelEventHandler<AnswersRemoved>, 
        ICompositeQuestionWithChildren,
        IDisposable
    {
        const double jsonSerializerDecimalLimit = 9999999999999999;
        
        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IViewModelEventRegistry liteEventRegistry;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ThrottlingViewModel throttlingModel;
        private readonly SpecialValuesViewModel specialValues;
        private Identity questionIdentity;
        private string interviewId;

        public IQuestionStateViewModel QuestionState => this.questionState; 
        public SpecialValuesViewModel SpecialValues => this.specialValues;

        public AnsweringViewModel Answering { get; }
        public QuestionInstructionViewModel InstructionViewModel { get; set; }

        private double? answer;
        public double? Answer
        {
            get => this.answer;
            set
            {
                if (this.answer != value)
                {
                    this.answer = value; 
                    this.RaisePropertyChanged();
                }
            }
        }

        public IMvxAsyncCommand ValueChangeCommand => new MvxAsyncCommand(this.SendAnswerRealQuestionCommand, () => this.principal.IsAuthenticated);

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswer);

        private readonly QuestionStateViewModel<NumericRealQuestionAnswered> questionState;

        private async Task RemoveAnswer()
        {
            try
            {
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
        
        public bool UseFormatting { get; set; }
        public int? CountOfDecimalPlaces { get; private set; }

        public RealQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericRealQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IQuestionnaireStorage questionnaireRepository, 
            IViewModelEventRegistry liteEventRegistry, 
            SpecialValuesViewModel specialValues, 
            ThrottlingViewModel throttlingModel)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;

            this.questionState = questionStateViewModel;
            this.Answering = answering;
            this.InstructionViewModel = instructionViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.liteEventRegistry = liteEventRegistry;
            this.specialValues = specialValues;
            this.throttlingModel = throttlingModel;
            this.throttlingModel.Init(SaveAnswer);
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionIdentity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));

            this.liteEventRegistry.Subscribe(this, interviewId);
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.UseFormatting = questionnaire.ShouldUseFormatting(entityIdentity.Id);
            this.CountOfDecimalPlaces = questionnaire.GetCountOfDecimalPlacesAllowedByQuestion(entityIdentity.Id);

            var doubleQuestion = interview.GetDoubleQuestion(entityIdentity);
            if (doubleQuestion.IsAnswered())
            {
                this.Answer = doubleQuestion.GetAnswer().Value;
            }

            InitSpecialValues(interviewId, entityIdentity);

            if (!this.specialValues.HasSpecialValues)
                this.throttlingModel.ThrottlePeriod = 0;
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

        private async Task SendAnswerRealQuestionCommand()
        {
            if (this.Answer == null)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                    .Interview_Question_Integer_EmptyValueError);
                return;
            }

            if (this.Answer > jsonSerializerDecimalLimit || this.Answer < -jsonSerializerDecimalLimit)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Real_ParsingError);
                return;
            }

            var answeredOrSelectedValue = Convert.ToDecimal(this.Answer.Value);
            await EnqueueSaveAnswer(answeredOrSelectedValue, specialValues.IsSpecialValueSelected(answeredOrSelectedValue));
        }

        private async Task EnqueueSaveAnswer(decimal answeredOrSelectedValue, bool isSpecialValueSelected)
        {
            this.answeredOrSelectedValueToSave = answeredOrSelectedValue;
            this.isSpecialValueSelectedToSave = isSpecialValueSelected;

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        decimal? answeredOrSelectedValueToSave = null;
        bool isSpecialValueSelectedToSave = false;

        private async Task SaveAnswer()
        {
            decimal? answeredOrSelectedValue = this.answeredOrSelectedValueToSave;
            bool isSpecialValueSelected = this.isSpecialValueSelectedToSave;

            var command = new AnswerNumericRealQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answer: Convert.ToDouble(answeredOrSelectedValue.Value));
            
            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();

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

        private bool isDisposed;
        public void Dispose()
        {
            if (this.isDisposed) return;

            this.isDisposed = true;

            this.liteEventRegistry.Unsubscribe(this); 
            this.QuestionState.Dispose();

            this.specialValues.SpecialValueChanged -= SpecialValueChanged;
            this.specialValues.SpecialValueRemoved -= SpecialValueRemoved;
            this.specialValues.Dispose();
            this.throttlingModel.Dispose();
        }

        public async Task Handle(AnswersRemoved @event)
        {
            if (this.isDisposed) return;

            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.Answer = null;
                    await this.specialValues.ClearSelectionAndShowValues();
                }
            }
        }

        public IObservableCollection<ICompositeEntity> Children => specialValues.AsChildren;
    }
}

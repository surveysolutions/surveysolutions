﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class CascadingSingleOptionQuestionViewModel : BaseComboboxQuestionViewModel, 
         ILiteEventHandler<SingleOptionQuestionAnswered>
    {
        private readonly ThrottlingViewModel throttlingModel;

        private readonly IQuestionnaireStorage questionnaireRepository;

        private Identity parentQuestionIdentity;
        private int? answerOnParentQuestion;

        private bool showCascadingAsList;
        private int showCascadingAsListThreshold;

        private int defaultCascadingAsListThreshold = 50;

        public CascadingSingleOptionQuestionViewModel(IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry, 
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel, 
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher,
            FilteredOptionsViewModel filteredOptionsViewModel,
            ThrottlingViewModel throttlingModel) :
            base(principal: principal, questionStateViewModel: questionStateViewModel, answering: answering,
                instructionViewModel: instructionViewModel, interviewRepository: interviewRepository,
                eventRegistry: eventRegistry, filteredOptionsViewModel)
        {
            this.questionnaireRepository = questionnaireRepository ??
                                           throw new ArgumentNullException(nameof(questionnaireRepository));

            this.Options = new CovariantObservableCollection<SingleOptionQuestionOptionViewModel>();
            
            this.mvxMainThreadDispatcher = mvxMainThreadDispatcher;
            
            this.throttlingModel = throttlingModel;
            this.throttlingModel.Init(SaveAnswer);
        }

        public override void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            base.Init(interviewId, entityIdentity, navigationState);

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            showCascadingAsList = questionnaire.ShowCascadingAsList(entityIdentity.Id);
            showCascadingAsListThreshold = Math.Min(defaultCascadingAsListThreshold, questionnaire.GetCascadingAsListThreshold(entityIdentity.Id) ?? defaultCascadingAsListThreshold);

            var cascadingQuestionParentId = questionnaire.GetCascadingQuestionParentId(entityIdentity.Id);
            if (!cascadingQuestionParentId.HasValue) throw new NullReferenceException($"Parent of cascading question {entityIdentity} is missing");
            
            var parentRosterVector = entityIdentity.RosterVector.Take(questionnaire.GetRosterLevelForEntity(cascadingQuestionParentId.Value)).ToArray();

            this.parentQuestionIdentity = new Identity(cascadingQuestionParentId.Value, parentRosterVector);

            var parentSingleOptionQuestion = interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
            if (parentSingleOptionQuestion.IsAnswered())
            {
                this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
            }
            
            this.filteredOptionsViewModel.ParentValue = this.answerOnParentQuestion;
            SetAnswerAndUpdateFilter();

            UpdateOptions();
        }

        public override async Task SaveAnswerAsync(int optionValue)
        {
            if (!this.answerOnParentQuestion.HasValue)
                return;

            await base.SaveAnswerAsync(optionValue);
        }

        public void Handle(SingleOptionQuestionAnswered @event)
        {
            if (!this.parentQuestionIdentity.Equals(@event.QuestionId, @event.RosterVector)) return;

            var parentSingleOptionQuestion = this.interview.GetSingleOptionQuestion(this.parentQuestionIdentity);
            if (!parentSingleOptionQuestion.IsAnswered()) return;

            this.answerOnParentQuestion = parentSingleOptionQuestion.GetAnswer().SelectedValue;
            this.filteredOptionsViewModel.ParentValue = this.answerOnParentQuestion;

            this.RaisePropertyChanged(() => RenderAsComboBox);
            this.UpdateOptions();

            this.RaisePropertyChanged(() => Options);
            this.RaisePropertyChanged(() => Children);
        }

        public bool RenderAsComboBox
        {
            get {

                if (!showCascadingAsList || answerOnParentQuestion == null)
                    return true;
                
                return interview.DoesCascadingQuestionHaveMoreOptionsThanThreshold(this.Identity, showCascadingAsListThreshold);
            }
        }

        public override IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();

                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(Options);
                result.AddCollection(comboboxCollection);
                result.Add(this.optionsBottomBorderViewModel);

                return result;
            }
        }

        public CovariantObservableCollection<SingleOptionQuestionOptionViewModel> Options { get; private set; }

        private async Task UpdateOptions()
        {
            this.Options.ForEach(x => x.BeforeSelected -= this.OptionSelected);
            this.Options.ForEach(x => x.AnswerRemoved -= this.RemoveAnswer);

            this.Options.ForEach(x => x.DisposeIfDisposable());
            this.Options.Clear();

            await this.comboboxViewModel.UpdateFilter(null);
            this.comboboxCollection.Remove(this.comboboxViewModel);

            if (!RenderAsComboBox)
            {
                var singleOptionQuestionOptionViewModels = filteredOptionsViewModel.GetOptions()
                    .Select(model => this.ToViewModel(model, isSelected: Answer.HasValue && model.Value == Answer.Value))
                    .ToList();

                singleOptionQuestionOptionViewModels.ForEach(x => this.Options.Add(x));
            }
            else
            {
                this.comboboxCollection.Add(this.comboboxViewModel);
            }
        }

        private SingleOptionQuestionOptionViewModel ToViewModel(CategoricalOption model, bool isSelected)
        {
            var optionViewModel = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.QuestionState.Enablement,
                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
                QuestionState = this.QuestionState,
            };
            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;

            return optionViewModel;
        }

        private async Task OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel)sender;
            selectedOptionToSave = selectedOption.Value;

            this.Options.Where(x => x.Selected && x.Value != selectedOptionToSave).ForEach(x => x.Selected = false);

            await this.throttlingModel.ExecuteActionIfNeeded();
        }

        private async Task RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                this.throttlingModel.CancelPendingAction();
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.principal.CurrentUserIdentity.UserId,
                        this.Identity));
                this.QuestionState.Validity.ExecutedWithoutExceptions();

                foreach (var option in this.Options.Where(option => option.Selected).ToList())
                {
                    option.Selected = false;
                }

                this.previousOptionToReset = null;
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }


        private int? previousOptionToReset = null;
        private int? selectedOptionToSave = null;
        private readonly IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;

        private async Task SaveAnswer()
        {
            if (this.selectedOptionToSave == this.previousOptionToReset)
                return;

            var selectedOption = this.GetOptionByValue(this.selectedOptionToSave);
            var previousOption = this.GetOptionByValue(this.previousOptionToReset);

            if (selectedOption == null)
                return;
            
            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.principal.CurrentUserIdentity.UserId,
                this.Identity.Id,
                this.Identity.RosterVector,
                selectedOption.Value);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendAnswerQuestionCommandAsync(command).ConfigureAwait(false);

                this.previousOptionToReset = this.selectedOptionToSave;

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                selectedOption.Selected = false;

                if (previousOption != null)
                {
                    previousOption.Selected = true;
                }

                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private SingleOptionQuestionOptionViewModel GetOptionByValue(int? value)
        {
            return value.HasValue
                ? this.Options.FirstOrDefault(x => x.Value == value.Value)
                : null;
        }

        public override void Dispose()
        {
            foreach (var option in this.Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
            }
            this.throttlingModel.Dispose();
            this.Options.ForEach(x => x.DisposeIfDisposable());

            base.Dispose();
        }
    }
}

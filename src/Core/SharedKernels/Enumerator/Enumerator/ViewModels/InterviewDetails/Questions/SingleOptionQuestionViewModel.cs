﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel, 
        IDisposable,
        ILiteEventHandler<AnswerRemoved>
    {
        private readonly Guid userId;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ILiteEventRegistry eventRegistry;

        public SingleOptionQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        private Identity questionIdentity;
        private Guid interviewId;

        public IList<SingleOptionQuestionOptionViewModel> Options { get; private set; }
        public QuestionStateViewModel<SingleOptionQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public bool HasOptions
        {
            get { return true; }
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetSingleOptionQuestion(entityIdentity.Id);
            var answerModel = interview.GetSingleOptionAnswer(entityIdentity);
            var selectedValue = Monads.Maybe(() => answerModel.Answer);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Options = questionModel
                .Options
                .Select(model => this.ToViewModel(model, isSelected: model.Value == selectedValue))
                .ToList();
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionQuestionOptionViewModel)sender;
            var previousOption = this.Options.SingleOrDefault(option => option.Selected && option != selectedOption);

            var command = new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedOption.Value);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendAnswerQuestionCommandAsync(command);

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

        private SingleOptionQuestionOptionViewModel ToViewModel(OptionModel model, bool isSelected)
        {
            var optionViewModel = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.QuestionState.Enablement,

                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
            };
            optionViewModel.QuestionState = this.QuestionState;
            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;

            return optionViewModel;
        }

        private async void RemoveAnswer(object sender, EventArgs e)
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.questionIdentity,
                        DateTime.UtcNow));
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        public void Handle(AnswerRemoved @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.RosterVector.SequenceEqual(this.questionIdentity.RosterVector))
            {
                foreach (var option in this.Options.Where(option => option.Selected))
                {
                    option.Selected = false;
                }
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this, interviewId.FormatGuid());
            this.QuestionState.Dispose();
            foreach (var option in Options)
            {
                option.BeforeSelected -= this.OptionSelected;
                option.AnswerRemoved -= this.RemoveAnswer;
            }
        }
    }
}
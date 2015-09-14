﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<TextQuestionAnswered>,
        ILiteEventHandler<AnswerRemoved>
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public event EventHandler AnswerRemoved;

        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<TextQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public TextQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<TextQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.Answering = answering;
            this.QuestionState = questionStateViewModel;
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.liteEventRegistry.Subscribe(this, interviewId);

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            this.InitQuestionSettings();
            this.UpdateSelfFromModel();
        }

        private string answer;
        public string Answer
        {
            get { return this.answer; }
            set
            {
                this.answer = value;
                this.RaisePropertyChanged();
            }
        }

        private string mask;
        public string Mask
        {
            get { return this.mask; }
            private set { this.mask = value; this.RaisePropertyChanged(); this.RaisePropertyChanged(() => this.Hint); }
        }

        public string Hint
        {
            get
            {
                if (this.Mask.IsNullOrEmpty()) 
                    return UIResources.TextQuestion_Hint;

                string maskHint = this.Mask.Replace('*', '_').Replace('#', '_').Replace('~', '_');
                return UIResources.TextQuestion_MaskHint.FormatString(maskHint);
            }
        }

        public bool IsMaskedQuestionAnswered { get; set; }


        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand<string>(this.SendAnswerTextQuestionCommand)); }
        }

        private IMvxCommand answerRemoveCommand;

        public IMvxCommand RemoveAnswerCommand
        {
            get
            {
                return this.answerRemoveCommand ??
                       (this.answerRemoveCommand = new MvxCommand(async () => await this.RemoveAnswer()));
            }
        }

        private async Task RemoveAnswer()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                removeTime: DateTime.UtcNow);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }

            if (this.AnswerRemoved != null) this.AnswerRemoved.Invoke(this, EventArgs.Empty);
        }

        private async void SendAnswerTextQuestionCommand(string text)
        {
            if (!this.Mask.IsNullOrEmpty() && !this.IsMaskedQuestionAnswered)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Text_MaskError);
                return;
            }

            var command = new AnswerTextQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: text);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void InitQuestionSettings()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var textQuestionModel = questionnaire.GetTextQuestion(this.questionIdentity.Id);
            this.Mask = textQuestionModel.Mask;
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);

            var answerModel = interview.GetTextAnswer(this.questionIdentity);
            this.Answer = answerModel.Answer;
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.RosterVector.SequenceEqual(this.questionIdentity.RosterVector))
            {
                this.UpdateSelfFromModel();
            }
        }

        public void Handle(AnswerRemoved @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.RosterVector.SequenceEqual(this.questionIdentity.RosterVector))
            {
                this.UpdateSelfFromModel();
            }
        }
    }
}
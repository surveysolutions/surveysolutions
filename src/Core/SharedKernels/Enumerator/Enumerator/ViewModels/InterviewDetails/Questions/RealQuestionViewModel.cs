﻿using System;
using System.Globalization;
using Cirrious.MvvmCross.ViewModels;
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
    public class RealQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel, IDisposable
    {
        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericRealQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        private string answerAsString;

        private int? countOfDecimalPlaces;

        public string AnswerAsString
        {
            get { return this.answerAsString; }
            private set
            {
                if (this.answerAsString != value)
                {
                    this.answerAsString = value; 
                    this.RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand valueChangeCommand;

        public IMvxCommand ValueChangeCommand
        {
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand(this.SendAnswerRealQuestionCommand)); }
        }

        public int? CountOfDecimalPlaces
        {
            get { return this.countOfDecimalPlaces; }
            set { this.countOfDecimalPlaces = value; this.RaisePropertyChanged(); }
        }

        public RealQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericRealQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering, 
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
            this.questionnaireRepository = questionnaireRepository;
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetRealNumericAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetRealNumericQuestion(entityIdentity.Id);

            this.CountOfDecimalPlaces = questionModel.CountOfDecimalPlaces;
            if (answerModel.IsAnswered)
            {
                this.AnswerAsString = NullableDecimalToAnswerString(answerModel.Answer);
            }
        }

        private async void SendAnswerRealQuestionCommand()
        {
            if (string.IsNullOrWhiteSpace(this.AnswerAsString))
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_EmptyValueError);
                return;
            }

            decimal answer;
            if (!Decimal.TryParse(this.AnswerAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out answer))
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Real_ParsingError);
                return;
            }

            var command = new AnswerNumericRealQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: answer);

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

        private static string NullableDecimalToAnswerString(decimal? answer)
        {
            return answer.HasValue ? answer.Value.ToString(CultureInfo.InvariantCulture) : null;
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
        }
    }
}
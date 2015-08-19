﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class IntegerQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        const int RosterUpperBoundDefaultValue = 40;

        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionService userInteractionService;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericIntegerQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        private bool isRosterSizeQuestion;

        private int? previousAnswer;
        private int answerMaxValue;

        private string answerAsString;
        public string AnswerAsString
        {
            get { return this.answerAsString; }
            set
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
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand(async () => await this.SendAnswerIntegerQuestionCommandAsync())); }
        }

        public IntegerQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericIntegerQuestionAnswered> questionStateViewModel,
            IUserInteractionService userInteractionService,
            AnsweringViewModel answering)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteractionService = userInteractionService;
            this.Answering = answering;
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
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetIntegerNumericQuestion(entityIdentity.Id);

            if (answerModel.IsAnswered)
            {
                var answer = answerModel.Answer;
                this.AnswerAsString = NullableIntToAnswerString(answer);
                this.previousAnswer = Monads.Maybe(() => answer);
            }
            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;
            this.answerMaxValue = RosterUpperBoundDefaultValue;
        }

        private async Task SendAnswerIntegerQuestionCommandAsync()
        {
            if (string.IsNullOrWhiteSpace(this.AnswerAsString))
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_EmptyValueError);
                return;
            }

            int answer;
            if (!int.TryParse(this.AnswerAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out answer))
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_Integer_ParsingError);
                return;
            }

            if (this.isRosterSizeQuestion)
            {
                if (answer < 0)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_NegativeRosterSizeAnswer, this.AnswerAsString);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (answer > this.answerMaxValue)
                {
                    var message = string.Format(UIResources.Interview_Question_Integer_RosterSizeAnswerMoreThanMaxValue, this.AnswerAsString, this.answerMaxValue);
                    this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(message);
                    return;
                }

                if (this.previousAnswer.HasValue && answer < this.previousAnswer)
                {
                    var amountOfRostersToRemove = this.previousAnswer - answer;
                    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                    if (!(await this.userInteractionService.ConfirmAsync(message)))
                    {
                        this.AnswerAsString = NullableIntToAnswerString(this.previousAnswer);
                        return;
                    }
                }
            }

            var command = new AnswerNumericIntegerQuestionCommand(
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

                this.previousAnswer = answer;
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private static string NullableIntToAnswerString(int? answer)
        {
            return answer.HasValue ? answer.Value.ToString(CultureInfo.InvariantCulture) : null;
        }
    }
}
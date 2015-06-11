using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class SingleOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly Guid userId;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAnswerToStringService answerToStringService;

        public SingleOptionLinkedQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.answerToStringService = answerToStringService;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        private Identity questionIdentity;
        private Guid interviewId;

        public ObservableCollection<SingleOptionLinkedQuestionOptionViewModel> Options { get; private set; }
        public QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.InitOptionsFromModel(interview, questionnaire);
        }

        private void InitOptionsFromModel(IStatefulInterview interview, QuestionnaireModel questionnaire)
        {
            var linkedQuestionModel = questionnaire.GetLinkedSingleOptionQuestion(this.questionIdentity.Id);
            var linkedAnswerModel = interview.GetLinkedSingleOptionAnswer(this.questionIdentity);

            IEnumerable<BaseInterviewAnswer> referencedQuestionAnswers =
                interview.FindBaseAnswerByOrShorterRosterLevel(linkedQuestionModel.LinkedToQuestionId, this.questionIdentity.RosterVector);

            var referencedQuestion = questionnaire.Questions[linkedQuestionModel.LinkedToQuestionId];

            var options = referencedQuestionAnswers
                .Select(referencedAnswer => this.GenerateOptionViewModelOrNull(referencedAnswer, referencedQuestion, linkedAnswerModel))
                .Where(optionOrNull => optionOrNull != null)
                .ToList();

            this.Options = new ObservableCollection<SingleOptionLinkedQuestionOptionViewModel>(options);
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            var selectedOption = (SingleOptionLinkedQuestionOptionViewModel)sender;
            var previousOption = this.Options.SingleOrDefault(option => option.Selected && option != selectedOption);

            var command = new AnswerSingleOptionLinkedQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedOption.RosterVector);

            try
            {
                if (previousOption != null)
                {
                    previousOption.Selected = false;
                }

                await this.Answering.SendAnswerQuestionCommand(command);

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

        private SingleOptionLinkedQuestionOptionViewModel GenerateOptionViewModelOrNull(BaseInterviewAnswer referencedAnswer, BaseQuestionModel referencedQuestion, LinkedSingleOptionAnswer linkedAnswerModel)
        {
            if (referencedAnswer == null || !referencedAnswer.IsAnswered)
                return null;

            string title = this.answerToStringService.AnswerToString(referencedQuestion, referencedAnswer);

            var isSelected =
                linkedAnswerModel != null &&
                linkedAnswerModel.IsAnswered &&
                linkedAnswerModel.Answer.SequenceEqual(referencedAnswer.RosterVector);

            var optionViewModel = new SingleOptionLinkedQuestionOptionViewModel
            {
                Enablement = this.QuestionState.Enablement,

                RosterVector = referencedAnswer.RosterVector,
                Title = title,
                Selected = isSelected,
            };

            optionViewModel.BeforeSelected += this.OptionSelected;

            return optionViewModel;
        }
    }
}
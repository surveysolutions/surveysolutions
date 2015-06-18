using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class SingleOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>
    {
        private readonly Guid userId;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAnswerToStringService answerToStringService;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;

        public SingleOptionLinkedQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireStorage == null) throw new ArgumentNullException("questionnaireStorage");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");
            if (answerToStringService == null) throw new ArgumentNullException("answerToStringService");
            if (eventRegistry == null) throw new ArgumentNullException("eventRegistry");

            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireStorage = questionnaireStorage;
            this.interviewRepository = interviewRepository;
            this.answerToStringService = answerToStringService;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher ?? MvxMainThreadDispatcher.Instance;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        private Identity questionIdentity;
        private Guid interviewId;
        private Guid referencedQuestionId;

        public ObservableCollection<SingleOptionLinkedQuestionOptionViewModel> Options { get; private set; }
        public QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            this.QuestionState.Init(interviewId, questionIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);

            this.questionIdentity = questionIdentity;
            this.interviewId = interview.Id;

            var questionModel = questionnaire.GetLinkedSingleOptionQuestion(this.questionIdentity.Id);
            this.referencedQuestionId = questionModel.LinkedToQuestionId;

            this.InitOptionsFromModel(interview, questionnaire);

            this.eventRegistry.Subscribe(this);
        }

        private void InitOptionsFromModel(IStatefulInterview interview, QuestionnaireModel questionnaire)
        {
            var linkedAnswerModel = interview.GetLinkedSingleOptionAnswer(this.questionIdentity);

            IEnumerable<BaseInterviewAnswer> referencedQuestionAnswers =
                interview.FindAnswersOfLinkedToQuestionForLinkedQuestion(this.referencedQuestionId, this.questionIdentity);

            var referencedQuestion = questionnaire.Questions[this.referencedQuestionId];

            var options = referencedQuestionAnswers
                .Select(referencedAnswer => this.GenerateOptionViewModelOrNull(referencedAnswer, referencedQuestion, linkedAnswerModel, interview))
                .Where(optionOrNull => optionOrNull != null)
                .ToList();

            this.Options = new ObservableCollection<SingleOptionLinkedQuestionOptionViewModel>(options);
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            await OptionSelectedImpl(sender);
        }

        internal async Task OptionSelectedImpl(object sender)
        {
            var selectedOption = (SingleOptionLinkedQuestionOptionViewModel) sender;
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

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (question.Id == this.referencedQuestionId)
                {
                    var optionToRemove = this.Options.SingleOrDefault(option => option.RosterVector.SequenceEqual(question.RosterVector));

                    if (optionToRemove != null)
                    {
                        this.mainThreadDispatcher.RequestMainThreadAction(() => this.Options.Remove(optionToRemove));
                    }
                }
            }
        }

        private SingleOptionLinkedQuestionOptionViewModel GenerateOptionViewModelOrNull(
            BaseInterviewAnswer referencedAnswer, BaseQuestionModel referencedQuestion,
            LinkedSingleOptionAnswer linkedAnswerModel, IStatefulInterview interview)
        {
            if (referencedAnswer == null || !referencedAnswer.IsAnswered)
                return null;

            var title = this.GenerateOptionTitle(referencedQuestion, referencedAnswer, interview);

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

        private string GenerateOptionTitle(BaseQuestionModel referencedQuestion, BaseInterviewAnswer referencedAnswer, IStatefulInterview interview)
        {
            string answerAsTitle = this.answerToStringService.AnswerToString(referencedQuestion, referencedAnswer);

            string rosterTitlesWithoutLast = string.Join(": ", interview.GetParentRosterTitlesWithoutLast(referencedAnswer.Id, referencedAnswer.RosterVector));

            return string.IsNullOrEmpty(rosterTitlesWithoutLast) ? answerAsTitle : string.Join(": ", rosterTitlesWithoutLast, answerAsTitle);
        }
    }
}
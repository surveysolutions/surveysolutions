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
    public class LinkedMultiOptionQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAnswerToStringService answerToStringService;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private readonly IPrincipal userIdentity;
        private Guid linkedToQuestionId;
        private int? maxAllowedAnswers;
        private Guid interviewId;
        private Guid userId;
        private Identity questionIdentity;
        private bool areAnswersOrdered;

        public QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public LinkedMultiOptionQuestionViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            IPrincipal userIdentity,
            ILiteEventRegistry eventRegistry)
        {
            this.interviewRepository = interviewRepository;
            this.answerToStringService = answerToStringService;
            this.questionnaireStorage = questionnaireStorage;
            this.userIdentity = userIdentity;
            this.QuestionState = questionState;
            this.Answering = answering;
            this.Options = new ObservableCollection<LinkedMultiOptionQuestionOptionViewModel>();
            eventRegistry.Subscribe(this);
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);
            LinkedMultiOptionQuestionModel linkedQuestionModel = questionnaire.GetLinkedMultiOptionQuestion(entityIdentity.Id);

            this.linkedToQuestionId = linkedQuestionModel.LinkedToQuestionId;
            this.maxAllowedAnswers = linkedQuestionModel.MaxAllowedAnswers;
            this.interviewId = interview.Id;
            this.userId = this.userIdentity.CurrentUserIdentity.UserId;
            this.questionIdentity = entityIdentity;
            this.areAnswersOrdered = linkedQuestionModel.AreAnswersOrdered;

            this.GenerateOptions(entityIdentity, interview, linkedQuestionModel, questionnaire);
        }

        public ObservableCollection<LinkedMultiOptionQuestionOptionViewModel> Options { get; private set; }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (question.Id == this.linkedToQuestionId)
                {
                    var shownAnswer = this.Options.SingleOrDefault(x => x.Value.SequenceEqual(question.RosterVector));
                    if (shownAnswer != null)
                    {
                        MvxMainThreadDispatcher.Instance.RequestMainThreadAction(() => this.Options.Remove(shownAnswer));
                    }
                }
            }
        }

        public async Task ToggleAnswerAsync(LinkedMultiOptionQuestionOptionViewModel changedModel)
        {
            List<LinkedMultiOptionQuestionOptionViewModel> allSelectedOptions = 
                this.areAnswersOrdered ? 
                this.Options.Where(x => x.Checked).OrderBy(x => x.CheckedTimeStamp).ThenBy(x => x.CheckedOrder).ToList() : 
                this.Options.Where(x => x.Checked).ToList();

            if (maxAllowedAnswers.HasValue && allSelectedOptions.Count > maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            decimal[][] selectedValues = allSelectedOptions
                .Select(x => x.Value)
                .ToArray();

            var command = new AnswerMultipleOptionsLinkedQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedValues);

            try
            {
                await this.Answering.SendAnswerQuestionCommand(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                changedModel.Checked = !changedModel.Checked;
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Handle(MultipleOptionsLinkedQuestionAnswered @event)
        {
            if (this.areAnswersOrdered)
            {
                this.PutOrderOnOptions(@event);
            }
        }

        private void GenerateOptions(Identity entityIdentity, 
            IStatefulInterview interview,
            LinkedMultiOptionQuestionModel linkedQuestionModel, 
            QuestionnaireModel questionnaire)
        {
            LinkedMultiOptionAnswer linkedMultiOptionAnswer = interview.GetLinkedMultiOptionAnswer(entityIdentity);
            IEnumerable<BaseInterviewAnswer> linkedQuestionAnswers =
                interview.FindBaseAnswerByOrShorterRosterLevel(linkedQuestionModel.LinkedToQuestionId, entityIdentity.RosterVector);

            this.Options.Clear();
            int checkedAnswerCount = 1;
            foreach (var answer in linkedQuestionAnswers)
            {
                if (answer != null && answer.IsAnswered)
                {
                    string title = this.answerToStringService.AnswerToString(questionnaire.Questions[entityIdentity.Id], answer);

                    var isChecked = linkedMultiOptionAnswer != null &&
                                    linkedMultiOptionAnswer.IsAnswered &&
                                    linkedMultiOptionAnswer.Answers.Any(x => x.SequenceEqual(answer.RosterVector));

                    var option = new LinkedMultiOptionQuestionOptionViewModel(this)
                    {
                        Title = title,
                        Value = answer.RosterVector,
                        Checked = isChecked
                    };
                    if (this.areAnswersOrdered && isChecked)
                    {
                        option.CheckedOrder = checkedAnswerCount;
                        checkedAnswerCount++;
                    }
                    this.Options.Add(option);
                }
            }
        }

        private void PutOrderOnOptions(MultipleOptionsLinkedQuestionAnswered @event)
        {
            var orderedSelectedOptions =
                this.Options.Where(x => @event.SelectedPropagationVectors.Any(y => y.SequenceEqual(x.Value)))
                    .OrderBy(x => x.CheckedTimeStamp)
                    .ToList();

            for (int i = 0; i < orderedSelectedOptions.Count; i++)
            {
                orderedSelectedOptions[i].CheckedOrder = i + 1;
            }

            foreach (var option in this.Options.Except(orderedSelectedOptions))
            {
                option.CheckedOrder = null;
            }
        }
    }
}
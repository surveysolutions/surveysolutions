using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultiOptionLinkedQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<MultipleOptionsLinkedQuestionAnswered>
    {
        private readonly AnswerNotifier answerNotifier;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAnswerToStringService answerToStringService;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private readonly IPrincipal userIdentity;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private Guid linkedToQuestionId;
        private int? maxAllowedAnswers;
        private Guid interviewId;
        private Guid userId;
        private Identity questionIdentity;
        private bool areAnswersOrdered;
        public QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public MultiOptionLinkedQuestionViewModel(QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            AnswerNotifier answerNotifier,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            IPrincipal userIdentity,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.answerNotifier = answerNotifier;
            this.interviewRepository = interviewRepository;
            this.answerToStringService = answerToStringService;
            this.questionnaireStorage = questionnaireStorage;
            this.userIdentity = userIdentity;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.QuestionState = questionState;
            this.Answering = answering;
            this.Options = new ObservableCollection<MultiOptionLinkedQuestionOptionViewModel>();
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

            this.answerNotifier.Init(this.linkedToQuestionId);

            this.answerNotifier.QuestionAnswered += this.LinkedToQuestionAnswered;
            this.GenerateOptions(interview, linkedQuestionModel, questionnaire);
        }

        private void LinkedToQuestionAnswered(object sender, EventArgs e)
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId.FormatGuid());
            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);
            LinkedMultiOptionQuestionModel linkedQuestionModel = questionnaire.GetLinkedMultiOptionQuestion(this.questionIdentity.Id);

            LinkedMultiOptionAnswer linkedMultiOptionAnswer = interview.GetLinkedMultiOptionAnswer(this.questionIdentity);
            List<BaseInterviewAnswer> linkedQuestionAnswers =
                interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedQuestionModel.LinkedToQuestionId, this.questionIdentity)
                .Where(x => x != null && x.IsAnswered).ToList();

            this.mainThreadDispatcher.RequestMainThreadAction(() => // otherwize its f.g magic with those observable collections. This is the only way I found to implement insertions without locks.
            {
                for (int i = 0; i < linkedQuestionAnswers.Count; i++)
                {
                    var linkedToQuestionModel = questionnaire.Questions[linkedQuestionModel.LinkedToQuestionId];
                    var linkedQuestionAnswer = linkedQuestionAnswers[i];

                    if (this.Options.Count > i && linkedQuestionAnswer.RosterVector.Identical(this.Options[i].Value))
                    {
                        var newTitle = this.answerToStringService.AnswerToString(linkedToQuestionModel, linkedQuestionAnswer);
                        this.Options[i].Title = newTitle;
                    }
                    else
                    {
                        var option = this.BuildOption(interview, linkedToQuestionModel, linkedQuestionAnswer, linkedMultiOptionAnswer); 
                        this.Options.Insert(i, option);
                    }
                }
            });
        }

        public ObservableCollection<MultiOptionLinkedQuestionOptionViewModel> Options { get; private set; }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (question.Id == this.linkedToQuestionId)
                {
                    var shownAnswer = this.Options.SingleOrDefault(x => x.Value.SequenceEqual(question.RosterVector));
                    if (shownAnswer != null)
                    {
                        this.InvokeOnMainThread(() => this.Options.Remove(shownAnswer));
                    }
                }
            }
        }

        public async Task ToggleAnswerAsync(MultiOptionLinkedQuestionOptionViewModel changedModel)
        {
            List<MultiOptionLinkedQuestionOptionViewModel> allSelectedOptions =
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
            if (this.areAnswersOrdered && @event.QuestionId == questionIdentity.Id && @event.PropagationVector.Identical(questionIdentity.RosterVector))
            {
                this.PutOrderOnOptions(@event);
            }
        }

        private void GenerateOptions(
            IStatefulInterview interview,
            LinkedMultiOptionQuestionModel linkedQuestionModel,
            QuestionnaireModel questionnaire)
        {
            LinkedMultiOptionAnswer thisQuestionAnswers = interview.GetLinkedMultiOptionAnswer(this.questionIdentity);
            IEnumerable<BaseInterviewAnswer> linkedToQuestionAnswers =
                interview.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedQuestionModel.LinkedToQuestionId, this.questionIdentity);
            this.Options.Clear();

            foreach (var answer in linkedToQuestionAnswers.Where(x => x.IsAnswered))
            {
                BaseQuestionModel linkedToQuestion = questionnaire.Questions[linkedQuestionModel.LinkedToQuestionId];
                var option = this.BuildOption(interview, linkedToQuestion, answer, thisQuestionAnswers);

                this.Options.Add(option);
            }
        }

        private MultiOptionLinkedQuestionOptionViewModel BuildOption(IStatefulInterview interview, 
            BaseQuestionModel linkedToQuestion, 
            BaseInterviewAnswer linkedToAnswer, 
            LinkedMultiOptionAnswer linkedMultiOptionAnswer)
        {
            var isChecked = linkedMultiOptionAnswer != null &&
                            linkedMultiOptionAnswer.IsAnswered &&
                            linkedMultiOptionAnswer.Answers.Any(x => x.Identical(linkedToAnswer.RosterVector));

            var title = this.BuildOptionTitle(interview, linkedToQuestion, linkedToAnswer);

            var option = new MultiOptionLinkedQuestionOptionViewModel(this)
            {
                Title = title,
                Value = linkedToAnswer.RosterVector,
                Checked = isChecked
            };
            if (this.areAnswersOrdered && isChecked)
            {
                int selectedItemIndex = Array.FindIndex(linkedMultiOptionAnswer.Answers, x => x.Identical(linkedToAnswer.RosterVector)) + 1;
                option.CheckedOrder = selectedItemIndex;
            }

            return option;
        }

        private string BuildOptionTitle(IStatefulInterview interview, BaseQuestionModel linkedToQuestion, BaseInterviewAnswer linkedToAnswer)
        {
            string answerAsTitle = this.answerToStringService.AnswerToString(linkedToQuestion, linkedToAnswer);

            int currentRosterLevel = this.questionIdentity.RosterVector.Length;

            IEnumerable<string> parentRosterTitlesWithoutLastOneAndFirstKnown =
                interview
                    .GetParentRosterTitlesWithoutLast(linkedToAnswer.Id, linkedToAnswer.RosterVector)
                    .Skip(currentRosterLevel);

            string rosterPrefixes = string.Join(": ", parentRosterTitlesWithoutLastOneAndFirstKnown);

            return string.IsNullOrEmpty(rosterPrefixes) ? answerAsTitle : string.Join(": ", rosterPrefixes, answerAsTitle);
        }

        private void PutOrderOnOptions(MultipleOptionsLinkedQuestionAnswered @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id && @event.PropagationVector.Identical(this.questionIdentity.RosterVector))
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
}
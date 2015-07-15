using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
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
        readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private Guid linkedToQuestionId;
        private int? maxAllowedAnswers;
        private Guid interviewId;
        private Guid userId;
        private Identity questionIdentity;
        private bool areAnswersOrdered;
        private ObservableCollection<MultiOptionLinkedQuestionOptionViewModel> options;
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
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.QuestionState = questionState;
            this.Answering = answering;
            this.Options = new ObservableCollection<MultiOptionLinkedQuestionOptionViewModel>();
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            eventRegistry.Subscribe(this, interviewId);

            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);
            LinkedMultiOptionQuestionModel linkedQuestionModel = questionnaire.GetLinkedMultiOptionQuestion(entityIdentity.Id);

            this.linkedToQuestionId = linkedQuestionModel.LinkedToQuestionId;
            this.maxAllowedAnswers = linkedQuestionModel.MaxAllowedAnswers;
            this.interviewId = interview.Id;
            this.userId = this.userIdentity.CurrentUserIdentity.UserId;
            this.questionIdentity = entityIdentity;
            this.areAnswersOrdered = linkedQuestionModel.AreAnswersOrdered;

            this.answerNotifier.Init(interviewId, this.linkedToQuestionId);

            this.answerNotifier.QuestionAnswered += this.LinkedToQuestionAnswered;
            this.Options = new ObservableCollection<MultiOptionLinkedQuestionOptionViewModel>(this.GenerateOptions(interview, questionnaire));
        }

        private void LinkedToQuestionAnswered(object sender, EventArgs e)
        {
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId.FormatGuid());
            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);
            
            var actualOptions = this.GenerateOptions(interview, questionnaire);

            this.mainThreadDispatcher.RequestMainThreadAction(() => // otherwize its f.g magic with those observable collections. This is the only way I found to implement insertions without locks.
            {
                List<MultiOptionLinkedQuestionOptionViewModel> optionsToRemove = this
                    .Options
                    .Where(existingOption => !actualOptions.Any(actualOption => actualOption.Value.Identical(existingOption.Value)))
                    .ToList();

                foreach (var optionToRemove in optionsToRemove)
                {
                    this.Options.Remove(optionToRemove);
                }

                for (int actualOptionIndex = 0; actualOptionIndex < actualOptions.Count; actualOptionIndex++)
                {
                    var actualOption = actualOptions[actualOptionIndex];
                    var existingOption = this.Options.SingleOrDefault(option => option.Value.Identical(actualOption.Value));

                    if (existingOption != null)
                    {
                        existingOption.Title = actualOption.Title;
                    }
                    else
                    {
                        this.Options.Insert(actualOptionIndex, actualOption);
                    }
                }

                this.RaisePropertyChanged(() => HasOptions);
            });
        }

        public ObservableCollection<MultiOptionLinkedQuestionOptionViewModel> Options
        {
            get { return this.options; }
            private set { this.options = value; this.RaisePropertyChanged(() => HasOptions);}
        }

        public bool HasOptions
        {
            get { return this.Options.Any(); }
        }

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
                        this.RaisePropertyChanged(() => HasOptions);
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
                await this.Answering.SendAnswerQuestionCommandAsync(command);
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

        private List<MultiOptionLinkedQuestionOptionViewModel> GenerateOptions(
            IStatefulInterview interview,
            QuestionnaireModel questionnaire)
        {
            LinkedMultiOptionAnswer thisQuestionAnswers = interview.GetLinkedMultiOptionAnswer(this.questionIdentity);
            IEnumerable<BaseInterviewAnswer> linkedToQuestionAnswers =
                interview.FindAnswersOfReferencedQuestionForLinkedQuestion(this.linkedToQuestionId, this.questionIdentity);

            List<MultiOptionLinkedQuestionOptionViewModel> options = new List<MultiOptionLinkedQuestionOptionViewModel>();
            foreach (var answer in linkedToQuestionAnswers)
            {
                BaseQuestionModel linkedToQuestion = questionnaire.Questions[this.linkedToQuestionId];
                var option = this.BuildOption(interview, linkedToQuestion, answer, thisQuestionAnswers);

                if (option != null)
                {
                    options.Add(option);
                }
            }
            return options;
        }

        private MultiOptionLinkedQuestionOptionViewModel BuildOption(IStatefulInterview interview,
            BaseQuestionModel linkedToQuestion,
            BaseInterviewAnswer linkedToAnswer,
            LinkedMultiOptionAnswer linkedMultiOptionAnswer)
        {
            var isChecked = linkedMultiOptionAnswer != null &&
                            linkedMultiOptionAnswer.IsAnswered &&
                            linkedMultiOptionAnswer.Answers.Any(x => x.Identical(linkedToAnswer.RosterVector));

            if (!linkedToAnswer.IsAnswered && !isChecked)
            {
                return null;
            }

            var title = this.BuildOptionTitle(interview, linkedToQuestion, linkedToAnswer);

            var option = new MultiOptionLinkedQuestionOptionViewModel(this)
            {
                Title = title,
                Value = linkedToAnswer.RosterVector,
                Checked = isChecked,
                QuestionState = this.QuestionState
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
            string answerAsTitle = this.answerToStringService.AnswerToUIString(linkedToQuestion, linkedToAnswer);

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
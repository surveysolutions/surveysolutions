using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class SingleOptionRosterLinkedQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswerRemoved>,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<RosterInstancesRemoved>,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        IDisposable
    {
        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IMvxMainThreadDispatcher mainThreadDispatcher;

        public SingleOptionRosterLinkedQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher,
            QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            if (principal == null) throw new ArgumentNullException("principal");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (eventRegistry == null) throw new ArgumentNullException("eventRegistry");

            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.eventRegistry = eventRegistry;
            this.mainThreadDispatcher = mainThreadDispatcher ?? MvxMainThreadDispatcher.Instance;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        private Identity questionIdentity;
        private Guid interviewId;
        private Guid referencedRosterId;
        private ObservableCollection<SingleOptionLinkedQuestionOptionViewModel> options;

        public ObservableCollection<SingleOptionLinkedQuestionOptionViewModel> Options
        {
            get { return this.options; }
            private set
            {
                this.options = value;
                this.RaisePropertyChanged(() => this.HasOptions);
            }
        }

        public bool HasOptions
        {
            get { return this.Options.Any(); }
        }

        public QuestionStateViewModel<SingleOptionLinkedQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public Identity Identity
        {
            get { return this.questionIdentity; }
        }

        public void Init(string interviewId, Identity questionIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            this.QuestionState.Init(interviewId, questionIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);

            this.questionIdentity = questionIdentity;
            this.interviewId = interview.Id;

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = questionnaire.GetLinkedToRosterSingleOptionQuestion(questionIdentity.Id);
            this.referencedRosterId = questionModel.LinkedToRosterId;

            this.Options =
                new ObservableCollection<SingleOptionLinkedQuestionOptionViewModel>(
                    this.GenerateOptionsFromModel(interview));

            this.eventRegistry.Subscribe(this, interviewId);
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

        private List<SingleOptionLinkedQuestionOptionViewModel> GenerateOptionsFromModel(IStatefulInterview interview)
        {
            var linkedAnswerModel = interview.GetLinkedSingleOptionAnswer(this.questionIdentity);

            IEnumerable<InterviewRoster> referencedRosters =
                interview.FindReferencedRostersForLinkedQuestion(this.referencedRosterId, this.questionIdentity);


            return referencedRosters
                .Select(
                    referencedRoster =>
                        this.GenerateOptionViewModelOrNull(referencedRoster, linkedAnswerModel, interview))
                .Where(o => o != null)
                .ToList();
        }

        private async void OptionSelected(object sender, EventArgs eventArgs)
        {
            await this.OptionSelectedAsync(sender);
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

        internal async Task OptionSelectedAsync(object sender)
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

        public void Handle(AnswerRemoved @event)
        {
            if (this.questionIdentity.Equals(@event.QuestionId, @event.RosterVector))
            {
                foreach (var option in this.Options.Where(option => option.Selected))
                {
                    option.Selected = false;
                }
                this.QuestionState.IsAnswered = false;
            }
        }

        private SingleOptionLinkedQuestionOptionViewModel GenerateOptionViewModelOrNull(
            InterviewRoster referencedRoster, LinkedSingleOptionAnswer linkedAnswerModel, IStatefulInterview interview)
        {
            if (referencedRoster == null)
                return null;

            var title = this.GenerateOptionTitle(referencedRoster, interview);

            if (string.IsNullOrEmpty(title))
                return null;

            var isSelected =
                linkedAnswerModel != null &&
                linkedAnswerModel.IsAnswered &&
                linkedAnswerModel.Answer.SequenceEqual(referencedRoster.RosterVector);

            return CreateSingleOptionLinkedQuestionOptionViewModel(title, isSelected, referencedRoster.RosterVector);
        }

        private SingleOptionLinkedQuestionOptionViewModel CreateSingleOptionLinkedQuestionOptionViewModel(string title,
            bool isSelected, decimal[] rosterVector)
        {
            var optionViewModel = new SingleOptionLinkedQuestionOptionViewModel
            {
                Enablement = this.QuestionState.Enablement,
                RosterVector = rosterVector,
                Title = title,
                Selected = isSelected,
                QuestionState = this.QuestionState
            };

            optionViewModel.BeforeSelected += this.OptionSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswer;
            return optionViewModel;
        }

        private string GenerateOptionTitle(InterviewRoster referencedRoster, IStatefulInterview interview)
        {
            string rosterTitle = referencedRoster.Title;
            int currentRosterLevel = this.questionIdentity.RosterVector.Length;

            IEnumerable<string> parentRosterTitlesWithoutLastOneAndFirstKnown =
                interview
                    .GetParentRosterTitlesWithoutLast(referencedRoster.Id, referencedRoster.RosterVector)
                    .Skip(currentRosterLevel);

            string rosterPrefixes = string.Join(": ", parentRosterTitlesWithoutLastOneAndFirstKnown);

            return string.IsNullOrEmpty(rosterPrefixes)
                ? rosterTitle
                : string.Join(": ", rosterPrefixes, rosterTitle);
        }

        public void Handle(AnswersRemoved @event)
        {

        }

        public void Handle(RosterInstancesRemoved @event)
        {
            foreach (var instancesToRemove in @event.Instances)
            {
                if (instancesToRemove.GroupId == this.referencedRosterId)
                {
                    var rosterVector =
                        instancesToRemove.OuterRosterVector.Union(new[]
                        {instancesToRemove.RosterInstanceId}).ToArray();

                    this.mainThreadDispatcher.RequestMainThreadAction(() =>
                    {
                        var optionToRemove =
                            this.Options.SingleOrDefault(
                                option => option.RosterVector.SequenceEqual(rosterVector));
                        if (optionToRemove != null)
                            this.Options.Remove(optionToRemove);

                        this.RaisePropertyChanged(() => this.HasOptions);
                    });
                }
            }
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            foreach (var changedInstances in @event.ChangedInstances)
            {
                if (changedInstances.RosterInstance.GroupId == this.referencedRosterId)
                {
                    var rosterVector =
                        changedInstances.RosterInstance.OuterRosterVector.Union(new[]
                        {changedInstances.RosterInstance.RosterInstanceId}).ToArray();

                    this.mainThreadDispatcher.RequestMainThreadAction(() =>
                    {
                        var optionToUpdate =
                            this.Options.SingleOrDefault(
                                option => option.RosterVector.SequenceEqual(rosterVector));
                        if (optionToUpdate == null)
                            this.Options.Add(CreateSingleOptionLinkedQuestionOptionViewModel(changedInstances.Title,
                                false, rosterVector));
                        else
                        {
                            optionToUpdate.Title = changedInstances.Title;
                        }

                        this.RaisePropertyChanged(() => this.HasOptions);
                    });
                }
            }
        }
    }
}
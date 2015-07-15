using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Repositories;
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
    public class MultiOptionQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPrincipal principal;
        private readonly Func<IUserInteraction> userInteraction;
        private Guid interviewId;
        private Identity questionIdentity;
        private Guid userId;
        private int? maxAllowedAnswers;
        private bool isRosterSizeQuestion;
        private bool areAnswersOrdered;

        public QuestionStateViewModel<MultipleOptionsQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public MultiOptionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewModel,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ILiteEventRegistry eventRegistry,
            IStatefulInterviewRepository interviewRepository,
            IPrincipal principal,
            Func<IUserInteraction> userInteraction,
            AnsweringViewModel answering)
        {
            this.Options = new ReadOnlyCollection<MultiOptionQuestionOptionViewModel>(new List<MultiOptionQuestionOptionViewModel>());
            this.QuestionState = questionStateViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.eventRegistry = eventRegistry;
            this.principal = principal;
            this.userInteraction = userInteraction;
            this.interviewRepository = interviewRepository;
            this.Answering = answering;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.eventRegistry.Subscribe(this, interviewId);
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            this.questionIdentity = entityIdentity;
            this.userId = principal.CurrentUserIdentity.UserId;
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;
            QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var questionModel = questionnaire.GetMultiOptionQuestion(entityIdentity.Id);

            this.areAnswersOrdered = questionModel.AreAnswersOrdered;
            this.maxAllowedAnswers = questionModel.MaxAllowedAnswers;
            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;

            MultiOptionAnswer existingAnswer = interview.GetMultiOptionAnswer(entityIdentity);
            this.Options = new ReadOnlyCollection<MultiOptionQuestionOptionViewModel>(questionModel.Options.Select((x, index) => ToViewModel(x, existingAnswer, index)).ToList());
        }

        public ReadOnlyCollection<MultiOptionQuestionOptionViewModel> Options { get; private set; }

        public bool HasOptions
        {
            get { return true; }
        }

        private MultiOptionQuestionOptionViewModel ToViewModel(OptionModel model, MultiOptionAnswer multiOptionAnswer, int answerIndex)
        {
            var result = new MultiOptionQuestionOptionViewModel(this)
            {
                Value = model.Value,
                Title = model.Title,
                Checked = multiOptionAnswer != null &&
                          multiOptionAnswer.IsAnswered &&
                          multiOptionAnswer.Answers.Any(x => model.Value == x)
            };
            result.CheckedOrder = this.areAnswersOrdered && result.Checked ? (int?)answerIndex + 1 : null;
            result.QuestionState = this.QuestionState;

            return result;
        }

        public async Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModel changedModel)
        {
            List<MultiOptionQuestionOptionViewModel> allSelectedOptions = 
                this.areAnswersOrdered ?
                this.Options.Where(x => x.Checked).OrderBy(x => x.CheckedOrder).ToList() :
                this.Options.Where(x => x.Checked).ToList();

            if (maxAllowedAnswers.HasValue && allSelectedOptions.Count > maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            if (this.isRosterSizeQuestion && !changedModel.Checked)
            {
                var amountOfRostersToRemove = 1;
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage,
                    amountOfRostersToRemove);
                if (!(await this.userInteraction().ConfirmAsync(message)))
                {
                    changedModel.Checked = true;
                    return;
                }
            }

            var selectedValues = allSelectedOptions.OrderBy(x => x.CheckedTimeStamp)
                .ThenBy(x => x.CheckedOrder)
                .Select(x => x.Value)
                .ToArray();

            var command = new AnswerMultipleOptionsQuestionCommand(
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

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if (this.areAnswersOrdered && @event.QuestionId == questionIdentity.Id && @event.PropagationVector.Identical(questionIdentity.RosterVector))
            {
                this.PutOrderOnOptions(@event);
            }
        }

        private void PutOrderOnOptions(MultipleOptionsQuestionAnswered @event)
        {
            var orderedOptions = this.Options.Where(x => @event.SelectedValues.Contains(x.Value))
                .OrderBy(x => x.CheckedTimeStamp)
                .ToList();

            for (int i = 0; i < orderedOptions.Count; i++)
            {
                orderedOptions[i].CheckedOrder = i + 1;
            }

            foreach (var option in this.Options.Except(orderedOptions))
            {
                option.CheckedOrder = null;
            }
        }
    }
}
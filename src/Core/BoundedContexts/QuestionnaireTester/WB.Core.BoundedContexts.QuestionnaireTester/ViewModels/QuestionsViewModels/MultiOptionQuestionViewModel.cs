using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultiOptionQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IPrincipal principal;
        private Guid interviewId;
        private Identity questionIdentity;
        private Guid userId;
        private int? maxAllowedAnswers;
        private bool isRosterSizeQuestion;

        public QuestionStateViewModel<MultipleOptionsQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }


        public MultiOptionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionStateViewModel,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            IPrincipal principal,
            AnsweringViewModel answering)
        {
            this.Options = new ReadOnlyCollection<MultiOptionQuestionOptionViewModel>(new List<MultiOptionQuestionOptionViewModel>());
            this.QuestionState = questionStateViewModel;
            this.questionnaireRepository = questionnaireRepository;
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.Answering = answering;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            this.questionIdentity = entityIdentity;

            this.userId = principal.CurrentUserIdentity.UserId;
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;
            QuestionnaireModel questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var questionModel = (MultiOptionQuestionModel)questionnaire.Questions[entityIdentity.Id];
            this.maxAllowedAnswers = questionModel.MaxAllowedAnswers;
            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;

            MultiOptionAnswer existingAnswer = interview.GetMultiOptionAnswer(entityIdentity);
            this.Options = new ReadOnlyCollection<MultiOptionQuestionOptionViewModel>(questionModel.Options.Select(x => ToViewModel(x, existingAnswer)).ToList());
        }

        public ReadOnlyCollection<MultiOptionQuestionOptionViewModel> Options { get; private set; }

        private MultiOptionQuestionOptionViewModel ToViewModel(OptionModel model, MultiOptionAnswer multiOptionAnswer)
        {
            var result = new MultiOptionQuestionOptionViewModel(this)
            {
                Value = model.Value,
                Title = model.Title,
                Checked = multiOptionAnswer != null && 
                          multiOptionAnswer.IsAnswered && 
                          multiOptionAnswer.Answers.Any(x => model.Value == x)
            };

            return result;
        }

        public async Task ToggleAnswer(MultiOptionQuestionOptionViewModel changedModel)
        {
            var allSelectedOptions = this.Options.Where(x => x.Checked).ToList();

            if (maxAllowedAnswers.HasValue && allSelectedOptions.Count > maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            if (this.isRosterSizeQuestion && !changedModel.Checked)
            {
                var amountOfRostersToRemove = 1;
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                if (!(await Mvx.Resolve<IUserInteraction>().ConfirmAsync(message)))
                {
                    changedModel.Checked = true;
                    return;
                }
            }

            var selectedValues = allSelectedOptions.Select(x => x.Value).ToArray();

            var command = new AnswerMultipleOptionsQuestionCommand(
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
    }
}
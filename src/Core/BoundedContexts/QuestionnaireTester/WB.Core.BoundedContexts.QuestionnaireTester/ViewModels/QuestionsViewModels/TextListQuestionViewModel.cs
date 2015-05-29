using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextListQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel
    {
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IUserInteraction userInteraction;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<TextListQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        private bool isRosterSizeQuestion;

        public ObservableCollection<TextListItemViewModel> Answers { get; set; }

        private string newListItem = string.Empty;
        public string NewListItem
        {
            get
            {
                return newListItem;
            }
            set
            {
                newListItem = value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.AddNewItemAndSaveAnswers(value.Trim());
                }
                this.RaisePropertyChanged();
            }
        }

        public TextListQuestionViewModel(
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            IUserInteraction userInteraction,
            AnsweringViewModel answering)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteraction = userInteraction;
            this.Answering = answering;
            this.Answers = new ObservableCollection<TextListItemViewModel>();
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetTextListAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (TextListQuestionModel)questionnaire.Questions[entityIdentity.Id];

            if (answerModel != null && answerModel.Answers != null)
            {
                answerModel.Answers.Select(this.ToViewModel).ForEach(x => this.Answers.Add(x));
            }

            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;
        }

        private async void ListItemEdited(object sender, EventArgs eventArgs)
        {
            //if (isRosterSizeQuestion && previousAnswer.HasValue && answer < previousAnswer)
            //{
            //    var amountOfRostersToRemove = previousAnswer - Math.Max(answer, 0);
            //    var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
            //    if (!(await userInteraction.ConfirmAsync(message)))
            //    {
            //        return;
            //    }
            //}

            var answers = this.Answers.Select(x => new Tuple<decimal, string>(x.Value, x.Title)).ToArray();

            var command = new AnswerTextListQuestionCommand(
                interviewId: Guid.Parse(interviewId),
                userId: principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answers: answers);

            try
            {
                await this.Answering.SendAnswerQuestionCommand(command);
                QuestionState.ExecutedAnswerCommandWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                QuestionState.ProcessAnswerCommandException(ex);
            }
        }

        private void AddNewItemAndSaveAnswers(string value)
        {
            var maxValue = Answers.Count == 0 ? 0 : Answers.Max(x => x.Value) + 1;

            Answers.Add(new TextListItemViewModel
            {
                Value = maxValue,
                Title = value
            });

            NewListItem = string.Empty;
        }

        private TextListItemViewModel ToViewModel(Tuple<decimal, string> model)
        {
            var optionViewModel = new TextListItemViewModel
            {
                Enablement = QuestionState.Enablement,

                Value = model.Item1,
                Title = model.Item2
            };

            optionViewModel.BeforeItemEdited += ListItemEdited;

            return optionViewModel;
        }
    }
}
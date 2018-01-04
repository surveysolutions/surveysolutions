﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextListQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel, 
        IDisposable, 
        ICompositeQuestionWithChildren
    {
        private readonly IPrincipal principal;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IUserInteractionService userInteractionService;
        private Identity questionIdentity;
        private string interviewId;
        private bool isRosterSizeQuestion;
        private int? maxAnswerCount;

        public QuestionInstructionViewModel InstructionViewModel { get; private set; }
        private readonly QuestionStateViewModel<TextListQuestionAnswered> questionState;
        private IMvxMainThreadDispatcher mainThreadDispatcher;

        private TextListAddNewItemViewModel addNewItemViewModel
            => this.Answers?.OfType<TextListAddNewItemViewModel>().FirstOrDefault();

        public IQuestionStateViewModel QuestionState => this.questionState;
        public AnsweringViewModel Answering { get; private set; }

        public CovariantObservableCollection<ICompositeEntity> Answers { get;  }
        
        public TextListQuestionViewModel(
            IPrincipal principal,
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<TextListQuestionAnswered> questionStateViewModel,
            IUserInteractionService userInteractionService,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.mainThreadDispatcher = mainThreadDispatcher;
            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.userInteractionService = userInteractionService;
            this.Answering = answering;
            this.Answers = new CovariantObservableCollection<ICompositeEntity>();
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.InstructionViewModel.Init(interviewId, entityIdentity);
            this.questionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.isRosterSizeQuestion = questionnaire.IsRosterSizeQuestion(this.questionIdentity.Id);
            this.maxAnswerCount = questionnaire.GetMaxSelectedAnswerOptions(this.questionIdentity.Id);

            var textListQuestion = interview.GetTextListQuestion(entityIdentity);
            if (textListQuestion.IsAnswered())
            {
                var answerViewModels = textListQuestion.GetAnswer().ToTupleArray().Select(x => this.CreateListItemViewModel(x.Item1, x.Item2));

                answerViewModels.ForEach(answerViewModel => this.Answers.Add(answerViewModel));
            }
            
            this.ShowOrHideAddNewItem();
        }

        private void ShowOrHideAddNewItem()
        {
            var answerVeiewModels = this.Answers.OfType<TextListItemViewModel>().ToList();

            bool denyAddNewItem = (this.maxAnswerCount.HasValue && answerVeiewModels.Count >= maxAnswerCount.Value) ||
                                  (this.isRosterSizeQuestion && answerVeiewModels.Count >= Constants.MaxLongRosterRowCount);

            if (denyAddNewItem && this.addNewItemViewModel != null)
            {
                this.addNewItemViewModel.ItemAdded -= this.ListItemAdded;
                this.Answers.Remove(addNewItemViewModel);
            }
            else if (!denyAddNewItem && this.addNewItemViewModel == null)
            {
                var viewModel = new TextListAddNewItemViewModel(this.questionState);
                viewModel.ItemAdded += this.ListItemAdded;
                this.Answers.Add(viewModel);
            }
        }

        private async void ListItemDeleted(object sender, EventArgs eventArgs)
        {
            var listItem = sender as TextListItemViewModel;

            if (listItem == null)
                return;

            if (this.isRosterSizeQuestion )
            {
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, 1);
                if (!(await this.userInteractionService.ConfirmAsync(message)))
                {
                    return;
                }
            }

            listItem.ItemEdited -= this.ListItemEdited;
            listItem.ItemDeleted -= this.ListItemDeleted;

            this.Answers.Remove(listItem);

            this.SaveAnswers();
        }

        private void ListItemAdded(object sender, TextListItemAddedEventArgrs e)
        {
            var answerViewModels = this.Answers.OfType<TextListItemViewModel>().ToList();
            var maxValue = answerViewModels.Count == 0
                ? 1
                : answerViewModels.Max(x => x.Value) + 1;

            this.Answers.Insert(this.Answers.Count - 1,
                this.CreateListItemViewModel(maxValue, e.NewText));

            this.SaveAnswers();

            if (this.addNewItemViewModel != null)
                this.addNewItemViewModel.Text = string.Empty;
        }

        private void ListItemEdited(object sender, EventArgs eventArgs) => this.SaveAnswers();

        private async void SaveAnswers()
        {
            var answerViewModels = this.Answers.OfType<TextListItemViewModel>().ToList();

            if (answerViewModels.Any(x => string.IsNullOrWhiteSpace(x.Title)))
            {
                this.questionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Interview_Question_List_Empty_Values_Are_Not_Allowed);
                return;
            }

            var answers = answerViewModels.Select(x => new Tuple<decimal, string>(x.Value, x.Title)).ToArray();

            var command = new AnswerTextListQuestionCommand(
                interviewId: Guid.Parse(this.interviewId),
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answers: answers);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.questionState.Validity.ExecutedWithoutExceptions();
                this.mainThreadDispatcher.RequestMainThreadAction(this.ShowOrHideAddNewItem);
            }
            catch (InterviewException ex)
            {
                this.questionState.Validity.ProcessException(ex);
            }
        }

        private TextListItemViewModel CreateListItemViewModel(decimal value, string title)
        {
            var optionViewModel = new TextListItemViewModel(this.questionState)
            {
                Value = value,
                Title = title
            };

            optionViewModel.ItemEdited += this.ListItemEdited;
            optionViewModel.ItemDeleted += this.ListItemDeleted;

            return optionViewModel;
        }

        public void Dispose() => this.questionState.Dispose();

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();                    
                result.Add(new OptionBorderViewModel<TextListQuestionAnswered>(this.questionState, true));
                result.AddCollection(this.Answers);
                result.Add(new OptionBorderViewModel<TextListQuestionAnswered>(this.questionState, false));
                return result;
            }
        }
    }
}
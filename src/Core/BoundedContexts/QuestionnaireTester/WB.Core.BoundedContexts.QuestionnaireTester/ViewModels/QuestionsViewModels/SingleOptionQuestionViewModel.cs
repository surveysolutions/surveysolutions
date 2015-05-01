using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class SingleOptionQuestionViewModel : MvxNotifyPropertyChanged, IInterviewItemViewModel
    {
        public QuestionHeaderViewModel Header { get; set; }

        private readonly ICommandService commandService;
        private readonly Guid userId;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;
        private Identity identity;
        private Guid interviewId;

        public SingleOptionQuestionViewModel(
            ICommandService commandService,
            IPrincipal principal,
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel)
        {
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.commandService = commandService;
            this.userId = principal.CurrentUserIdentity.UserId;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
        }

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            this.identity = questionIdentity;
            this.interviewId = interview.Id;

            var questionModel = (SingleOptionQuestionModel)questionnaire.Questions[this.identity.Id];
            var answerModel = interview.GetSingleAnswerModel(this.identity);

            this.Header.Init(interviewId, questionIdentity);
            this.Options = questionModel.Options.Select(ToViewModel).ToList();
            this.selectedOption = this.Options.SingleOrDefault(option => option.Value == Monads.Maybe(() => answerModel.Answer));

            if (answerModel != null)
            {
                Answer = answerModel.Answer;
            }
        }

        public IList<QuestionOptionViewModel> Options { get; private set; }

        public QuestionOptionViewModel SelectedOption
        {
            get { return selectedOption; }
            set
            {
                try
                {
                    this.ExecuteAnswerCommand(value);
                    selectedOption = value;
                }
                finally
                {
                    this.RaisePropertyChanged(() => SelectedOption);
                }
            }
        }

        private void ExecuteAnswerCommand(QuestionOptionViewModel option)
        {
            this.commandService.Execute(new AnswerSingleOptionQuestionCommand(
                this.interviewId,
                this.userId,
                this.identity.Id,
                this.identity.RosterVector,
                DateTime.Now,
                option.Value));
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }

        private decimal answer;
        private QuestionOptionViewModel selectedOption;

        public decimal Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(() => Answer); }
        }

        private static QuestionOptionViewModel ToViewModel(OptionModel model)
        {
            return new QuestionOptionViewModel
            {
                Value = model.Value,
                Title = model.Title,
                IsSelected = false,
            };
        }
    }
}
using System;

using Cirrious.MvvmCross.ViewModels;

using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class SingleOptionQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntity
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private IPlainRepository<InterviewModel> interviewRepository;
        private Identity identity;
        private Guid interviewId;

        public SingleOptionQuestionViewModel(
            ICommandService commandService,
            IPrincipal principal,
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository)
        {
            if (commandService == null) throw new ArgumentNullException("commandService");
            if (principal == null) throw new ArgumentNullException("principal");
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity identity)
        {
            if (identity == null) throw new ArgumentNullException("identity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            this.identity = identity;
            this.interviewId = interview.Id;

            var questionModel = (SingleOptionQuestionModel)questionnaire.Questions[this.identity.Id];
            var answerModel = interview.GetSingleAnswerModel(this.identity);

            if (answerModel != null)
            {
                Answer = answerModel.Answer;
            }
        }

        private decimal answer;
        public decimal Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(() => Answer); }
        }
    }
}
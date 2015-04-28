using System;

using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class IntegerNumericQuestionViewModel : MvxNotifyPropertyChanged, IInterviewItemViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private IPlainRepository<InterviewModel> interviewRepository;
        private Identity identity;
        private Guid interviewId;

        public IntegerNumericQuestionViewModel(
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

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            //var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            this.identity = questionIdentity;
            this.interviewId = interview.Id;

            //IntegerNumericQuestionModel questionModel = (IntegerNumericQuestionModel)questionnaire.Questions[this.identity.Id];
            var answerModel = interview.GetIntegerNumericAnswerModel(this.identity);

            if (answerModel != null)
            {
                Answer = answerModel.Answer;
            }
        }

        private long answer;
        public long Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(() => Answer); }
        }
    }
}
using System;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupReferenceViewModel : MvxNotifyPropertyChanged, IInterviewEntity
    {
        private IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private IPlainRepository<InterviewModel> interviewRepository;

        public GroupReferenceViewModel(
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository)
        {
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity identity)
        {
        }

        public bool IsComplete { get; set; }
        public int CountOfAnsweredQuestions { get; set; }
        public int CountOfCompletedGroups { get; set; }
        public bool IsDisabled { get; set; }

        private Guid id;

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }
    }
}
using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class QuestionHeaderViewModel : MvxNotifyPropertyChanged
    {
        public string Instruction { get; set; }
        public bool HasInstruction { get; set; }
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;

        public void Init(string interviewId, Identity questionIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);
            var questionModel = questionnaire.Questions[questionIdentity.Id];

            this.Title = questionModel.Title;
            this.Instruction = questionModel.Instructions;
            this.HasInstruction = !string.IsNullOrEmpty(questionModel.Instructions);
        }

        public QuestionHeaderViewModel(
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }
    }
}
using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, IInterviewEntity
    {
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;

        public StaticTextViewModel(IPlainRepository<QuestionnaireModel> questionnaireRepository,
             IPlainRepository<InterviewModel> interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity identity)
        {
            if (identity == null) throw new ArgumentNullException("identity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            this.StaticText = questionnaire.StaticTexts[identity.Id].Title;
        }

        private string staticText;
        public string StaticText
        {
            get { return staticText; }
            set { staticText = value; RaisePropertyChanged(); }
        }
    }
}
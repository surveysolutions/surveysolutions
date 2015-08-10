using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public StaticTextViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
             IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.questionIdentity = entityIdentity;

            this.StaticText = questionnaire.StaticTexts[entityIdentity.Id].Title;
        }

        private string staticText;
        private Identity questionIdentity;

        public string StaticText
        {
            get { return this.staticText; }
            set { this.staticText = value; this.RaisePropertyChanged(); }
        }
    }
}
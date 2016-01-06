using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public StaticTextViewModel(
            IQuestionnaireRepository questionnaireRepository,
            IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.questionIdentity = entityIdentity;

            this.StaticText = questionnaire.GetStaticText(entityIdentity.Id);
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
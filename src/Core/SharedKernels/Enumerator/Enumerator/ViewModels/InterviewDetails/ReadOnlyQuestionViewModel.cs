using System;
using System.Globalization;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class ReadOnlyQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel
    {
        public Identity Identity { get; private set; }

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public ReadOnlyQuestionViewModel(IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.Title = dynamicTextViewModel;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.Identity = entityIdentity;
            this.Title.Init(interviewId, entityIdentity);
            this.Answer = interview.GetQuestion(entityIdentity).GetAnswerAsString(CultureInfo.CurrentUICulture);
        }

        public DynamicTextViewModel Title { get; private set; }

        public string Answer { get; private set; }
    }
}
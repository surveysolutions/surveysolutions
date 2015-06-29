using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel, IInterviewAnchoredEntity
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public StaticTextViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
             IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.StaticText = questionnaire.StaticTexts[entityIdentity.Id].Title;
        }

        public int GetPositionOfAnchoredElement(Identity identity)
        {
            return -1;
        }

        private string staticText;
        public string StaticText
        {
            get { return staticText; }
            set { staticText = value; RaisePropertyChanged(); }
        }
    }
}
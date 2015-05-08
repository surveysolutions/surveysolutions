using System;
using Cirrious.MvvmCross.ViewModels;

using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class StaticTextViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

        public StaticTextViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
             IStatefullInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            this.StaticText = questionnaire.StaticTexts[entityIdentity.Id].Title;
        }

        private string staticText;
        public string StaticText
        {
            get { return staticText; }
            set { staticText = value; RaisePropertyChanged(); }
        }
    }
}
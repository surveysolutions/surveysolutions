using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class StaticTextViewModel : BaseInterviewItemViewModel
    {
        private Identity identity;
        private InterviewModel interviewModel;
        private QuestionnaireModel questionnaireModel;

        public StaticTextViewModel() { }

        public override void Init(Identity questionIdentity, InterviewModel interview, QuestionnaireModel questionnaire)
        {
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");
            if (interview == null) throw new ArgumentNullException("interview");
            if (questionnaire == null) throw new ArgumentNullException("questionnaire");

            this.identity = questionIdentity;
            this.interviewModel = interview;
            this.questionnaireModel = questionnaire;

            var staticText = this.questionnaireModel.StaticTexts[this.identity.Id];

            Title = staticText.Title;
          
        }

        public bool IsDisabled { get; set; }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }
    }
}
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

        public override void Init(Identity questionIdentity, InterviewModel interview, QuestionnaireModel questionnaireDocument)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            if (interviewModel == null) throw new ArgumentNullException("interviewModel");
            if (questionnaireModel == null) throw new ArgumentNullException("questionnaireModel");

            this.identity = identity;
            this.interviewModel = interviewModel;
            this.questionnaireModel = questionnaireModel;

            var staticText = this.questionnaireModel.StaticTexts[this.identity.Id];

            Title = staticText.Title;
          
        }

        public bool IsDisabled { get; set; }
        public Guid Id { get; set; }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }
    }
}
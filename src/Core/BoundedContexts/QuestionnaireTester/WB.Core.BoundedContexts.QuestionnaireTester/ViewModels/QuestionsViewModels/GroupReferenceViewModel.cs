using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupReferenceViewModel : BaseInterviewItemViewModel
    {
        public override void Init(Identity identity, InterviewModel interviewModel, QuestionnaireModel questionnaireModel)
        {
           
        }

        public bool IsComplete { get; set; }
        public int CountOfAnsweredQuestions { get; set; }
        public int CountOfCompletedGroups { get; set; }
        public bool IsDisabled { get; set; }

        private string title;
        private Guid id;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }
    }
}
using System;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupReferenceViewModel : MvxViewModel
    {
        public class NavObject
        {
            public Identity QuestionIdentity { get; set; }
            public InterviewModel InterviewModel { get; set; }
            public QuestionnaireDocument QuestionnaireDocument { get; set; }
        }

        public void Init(NavObject navObject)
        {
        }

        public void Init(Identity questionIdentity, InterviewModel interviewModel, QuestionnaireDocument questionnaireDocument)
        {
            Init(new NavObject 
            {
                QuestionIdentity = questionIdentity,
                InterviewModel = interviewModel,
                QuestionnaireDocument = questionnaireDocument
            });
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
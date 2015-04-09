using System;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using WB.Core.BoundedContexts.QuestionnaireTester.Model;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupReferanceViewModel : MvxViewModel
    {
        public GroupReferanceViewModel(Guid questionId, InterviewModel interviewModel, QuestionnaireDocument questionnaireDocument)
        {
        }


        private string title;
        private Guid id;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }

        private Guid Id
        {
            get { return id; }
            set { id = value; RaisePropertyChanged(() => Id); }
        }
    }
}
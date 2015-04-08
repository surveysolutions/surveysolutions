using System;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class GroupViewModel : MvxViewModel
    {
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
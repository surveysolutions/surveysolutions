using System;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.QuestionnaireTester.Model;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class StaticTextViewModel : MvxViewModel
    {
        public StaticTextViewModel() { }

        public StaticTextViewModel(Guid questionId, QuestionnaireDocument questionnaireDocument)
        {
            var staticText = questionnaireDocument.Find<IStaticText>(questionId);

            Title = staticText.Text;
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }
    }
}
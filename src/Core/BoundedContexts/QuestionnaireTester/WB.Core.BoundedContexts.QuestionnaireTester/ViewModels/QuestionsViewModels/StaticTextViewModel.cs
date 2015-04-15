using System;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class StaticTextViewModel : MvxViewModel
    {
        public class NavObject
        {
            public Identity QuestionIdentity { get; set; }
            public QuestionnaireDocument QuestionnaireDocument { get; set; }
        }

        public StaticTextViewModel() { }

        public void Init(NavObject navObject)
        {
            var staticText = navObject.QuestionnaireDocument.Find<IStaticText>(navObject.QuestionIdentity.Id);

            Title = staticText.Text;
        }

        public void Init(Identity questionIdentity, QuestionnaireDocument questionnaireDocument)
        {
            Init(new NavObject()
            {
                QuestionIdentity = questionIdentity,
                QuestionnaireDocument = questionnaireDocument
            });
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
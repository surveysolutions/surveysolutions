using System;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public abstract class AbstractQuestionnaireView
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public AbstractQuestionnaireView(IQuestionnaireDocument doc)
            : this()
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
        }

        public AbstractQuestionnaireView()
        {
        }
    }

    public abstract class AbstractQuestionnaireView<TGroup, TQuestion> : AbstractQuestionnaireView
        where TGroup : AbstractGroupView
        where TQuestion : AbstractQuestionView
    {

        public TQuestion[] Questions
        {
            get { return _questions; }
            set
            {
                _questions = value;
                for (int i = 0; i < this._questions.Length; i++)
                {
                    this._questions[i].Index = i + 1;
                }

            }
        }

        public TGroup[] Groups { get; set; }
        private TQuestion[] _questions;

        public AbstractQuestionnaireView(IQuestionnaireDocument doc)
            : base(doc)
        {
            
            this.Questions = new TQuestion[0];
            this.Groups = new TGroup[0];
        }

        public AbstractQuestionnaireView()
        {
            Questions = new TQuestion[0];
            Groups = new TGroup[0];
        }
    }

    public class QuestionnaireView :
        AbstractQuestionnaireView
            <GroupView,QuestionView>
    {
        public QuestionnaireView()
            : base()
        {
        }

        public QuestionnaireView(
            IQuestionnaireDocument doc)
            : base(doc)
        {
            this.Questions = doc.Children.OfType<IQuestion>().Select(q => new QuestionView(doc, q)).ToArray();
            this.Groups = doc.Children.OfType<IGroup>().Select(g => new GroupView(doc, g)).ToArray();
        }
    }
}

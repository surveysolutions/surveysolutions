using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public abstract class AbstractQuestionnaireView
    {
        public Guid PublicKey;
        public string Title { get; set; }
        public bool IsValid { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public AbstractQuestionnaireView(IQuestionnaireDocument doc): this()
        {
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
        }

        public AbstractQuestionnaireView(){}
    }

    public abstract class AbstractQuestionnaireView<TGroup, TQuestion> : AbstractQuestionnaireView
        where TGroup : AbstractGroupView
        where TQuestion : AbstractQuestionView, ICompositeView
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
        {
            this.Children = new List<ICompositeView>();
            this.Questions = new TQuestion[0];
            this.Groups = new TGroup[0];
            this.PublicKey = doc.PublicKey;
            this.Title = doc.Title;
        }

        public AbstractQuestionnaireView()
        {
            Children = new List<ICompositeView>();
        }

        public Guid PublicKey { get; set; }
        public string Title { get; set; }
        public Guid? Parent { get; set; }
        public List<ICompositeView> Children { get; set; }
    }

    public class QuestionnaireView : AbstractQuestionnaireView<GroupView,QuestionView>
    {
        public QuestionnaireView(): base(){}

        public QuestionnaireView(IQuestionnaireDocument doc): base(doc)
        {
            foreach (var composite in doc.Children)
            {
                if ((composite as IQuestion) != null)
                {
                    var q = composite as IQuestion;
                    List<IQuestion> r = doc.Children.OfType<IQuestion>().ToList();
                    this.Children.Add(new QuestionView(doc, q) { Index = r.IndexOf(q) });
                }
                else
                {
                    var g = composite as IGroup;
                    this.Children.Add(new GroupView(doc, g));

                }
            }
            this.Questions = doc.Children.OfType<IQuestion>().Select(q => new QuestionView(doc, q)).ToArray();
            this.Groups = doc.Children.OfType<IGroup>().Select(g => new GroupView(doc, g)).ToArray();
        }
    }
}

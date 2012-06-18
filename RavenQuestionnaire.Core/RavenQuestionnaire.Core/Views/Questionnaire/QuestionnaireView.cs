using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
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

    //public abstract class AbstractQuestionnaireView<TGroup, TQuestion> : AbstractQuestionnaireView
    //    where TGroup : AbstractGroupView
    //    where TQuestion : AbstractQuestionView
    //{

    //    public TQuestion[] Questions
    //    {
    //        get { return _questions; }
    //        set
    //        {
    //            _questions = value;
    //            for (int i = 0; i < this._questions.Length; i++)
    //            {
    //                this._questions[i].Index = i + 1;
    //            }

    //        }
    //    }

    //    public TGroup[] Groups { get; set; }
    //    private TQuestion[] _questions;

    //    public AbstractQuestionnaireView(IQuestionnaireDocument doc)
    //        : base(doc)
    //    {
            
    //        this.Questions = new TQuestion[0];
    //        this.Groups = new TGroup[0];
    //    }

    //    public AbstractQuestionnaireView()
    //    {
    //        Questions = new TQuestion[0];
    //        Groups = new TGroup[0];
    //    }
    //}

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
                //for (int i = 0; i < this._questions.Length; i++)
                //{
                //    this._questions[i].Index = i + 1;
                //}

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
            foreach (var composite in doc.Children)
            {
                if ((composite as IQuestion) != null)
                {
                    var q = composite as ICompleteQuestion;
                    var question = new CompleteQuestionFactory().CreateQuestion(doc as CompleteQuestionnaireDocument, null, q);
                    Children.Add(question);
                }
                else
                {
                    var g = composite as IGroup;
                    Children.Add(new GroupView(doc, g));

                }
            }
            this.Questions = doc.Children.OfType<IQuestion>().Select(q => new QuestionView(doc, q)).ToArray();
            this.Groups = doc.Children.OfType<IGroup>().Select(g => new GroupView(doc, g)).ToArray();
        }
    }
}

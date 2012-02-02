using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public abstract class AbstractQuestionnaireView<T> where T: AnswerView
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public AbstractFlowGraphView FlowGraph { get; set; }

        public AbstractQuestionView<T>[] Questions
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

        public AbstractGroupView<T>[] Groups { get; set; }
        private AbstractQuestionView<T>[] _questions;

        public AbstractQuestionnaireView(IQuestionnaireDocument doc)
            : this()
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Questions = new AbstractQuestionView<T>[0];
            this.Groups = new AbstractGroupView<T>[0];
        }

        /*  public AbstractQuestionnaireView(IQuestionnaireDocument<RavenQuestionnaire.Core.Entities.SubEntities.Group, RavenQuestionnaire.Core.Entities.SubEntities.Question> doc)
              : this((IQuestionnaireDocument)doc)
          {
              this.Questions = doc.Questions.Select(q => new QuestionView(doc, q)).ToArray();
              this.Groups = doc.Groups.Select(g => new GroupView(doc, g)).ToArray();
          }*/

        public AbstractQuestionnaireView()
        {
            Questions = new AbstractQuestionView<T>[0];
            Groups = new AbstractGroupView<T>[0];
            FlowGraph = null;
        }
    }

    public class QuestionnaireView<T, TGroup, TQuestion, TAnswer> : AbstractQuestionnaireView<T>
        where T: AnswerView
        where TAnswer : IAnswer
        where TQuestion : IQuestion<TAnswer>
        where TGroup : IGroup<TGroup, TQuestion>
    {
        public QuestionnaireView(IQuestionnaireDocument doc)
            : base()
        {
        }

        public QuestionnaireView(IQuestionnaireDocument<TGroup, TQuestion> doc)
            : base(doc)
        {
            /*   this.Questions = doc.Questions.Select(q => new QuestionView(doc, q)).ToArray();
               this.Groups = doc.Groups.Select(g => new GroupView(doc, g)).ToArray();*/
        }

        public QuestionnaireView()
            : base()
        {
        }
    }

    public class QuestionnaireView :
        QuestionnaireView
            <AnswerView,RavenQuestionnaire.Core.Entities.SubEntities.Group,
            RavenQuestionnaire.Core.Entities.SubEntities.Question,
            RavenQuestionnaire.Core.Entities.SubEntities.Answer>
    {
        public QuestionnaireView()
            : base()
        {
        }
        public QuestionnaireView(IQuestionnaireDocument doc)
            : base(doc)
        {
        }

        public QuestionnaireView(
            IQuestionnaireDocument
                <RavenQuestionnaire.Core.Entities.SubEntities.Group,
                RavenQuestionnaire.Core.Entities.SubEntities.Question> doc)
            : base(doc)
        {
            this.Questions = doc.Questions.Select(q => new QuestionView(doc, q)).ToArray();
            this.Groups = doc.Groups.Select(g => new GroupView(doc, g)).ToArray();
            this.FlowGraph = new FlowGraphView(doc as QuestionnaireDocument);
        }
    }
}

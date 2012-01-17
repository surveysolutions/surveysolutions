using System;
using System.Collections.Generic;
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
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public AbstractQuestionView[] Questions
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

        public AbstractGroupView[] Groups { get; set; }
        private AbstractQuestionView[] _questions;

        public AbstractQuestionnaireView(IQuestionnaireDocument doc)
            : this()
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.Title = doc.Title;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.Questions = new AbstractQuestionView[0];
            this.Groups = new AbstractGroupView[0];
        }

        /*  public AbstractQuestionnaireView(IQuestionnaireDocument<RavenQuestionnaire.Core.Entities.SubEntities.Group, RavenQuestionnaire.Core.Entities.SubEntities.Question> doc)
              : this((IQuestionnaireDocument)doc)
          {
              this.Questions = doc.Questions.Select(q => new QuestionView(doc, q)).ToArray();
              this.Groups = doc.Groups.Select(g => new GroupView(doc, g)).ToArray();
          }*/

        public AbstractQuestionnaireView()
        {
            Questions = new AbstractQuestionView[0];
            Groups = new AbstractGroupView[0];
        }

        public AbstractQuestionView[] GetQuestions(Guid? groupPublicKey)
        {
            if (!groupPublicKey.HasValue)
                return Questions;
            var group = Groups.FirstOrDefault(g => g.PublicKey.Equals(groupPublicKey.Value));
            if (group == null)
                throw new ArgumentException("group doesn't exists");
            return group.Questions;
        }
    }

    public class QuestionnaireView<TGroup, TQuestion, TAnswer> : AbstractQuestionnaireView
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
            <RavenQuestionnaire.Core.Entities.SubEntities.Group, RavenQuestionnaire.Core.Entities.SubEntities.Question,
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
        }
    }
}

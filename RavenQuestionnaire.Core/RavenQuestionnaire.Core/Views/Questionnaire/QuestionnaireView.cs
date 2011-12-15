using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Questionnaire
{
    public class QuestionnaireView
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate{ get; set; }

        public QuestionView[] Questions
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

        private QuestionView[] _questions;
        public QuestionnaireView(string id, string title, DateTime creationDate, DateTime lastEntryDate, IEnumerable<QuestionView> questions)
        {
            this.Id = IdUtil.ParseId(id);
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.Questions = questions.ToArray();
           
        }
        public QuestionnaireView()
        {
            Questions = new QuestionView[0];
        }

        public static QuestionnaireView New()
        {
            return new QuestionnaireView();
        }
    }
}

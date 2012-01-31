using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Documents
{
    public class CompleteQuestionDocument : ICompleteQuestion<CompleteAnswer>
    {
        protected CompleteQuestionDocument()
        {
            this.PublicKey = Guid.NewGuid();
            this.Enabled = true;
            Answers = new List<CompleteAnswer>();
        }

        protected CompleteQuestionDocument(string text, QuestionType type)
            : this()
        {

            this.QuestionText = text;
            this.QuestionType = type;
        }
        #region Implementation of IQuestion

        public Guid PublicKey { get; set; }

        public string QuestionText { get; set; }

        public QuestionType QuestionType { get; set; }

        public string ConditionExpression { get; set; }

        public string StataExportCaption { get; set; }

        #endregion

        #region Implementation of ICompleteQuestion

        public bool Enabled { get; set; }

        #endregion

        #region Implementation of IQuestion<CompleteAnswer>

        public List<CompleteAnswer> Answers { get; set; }

        #endregion
    }
}

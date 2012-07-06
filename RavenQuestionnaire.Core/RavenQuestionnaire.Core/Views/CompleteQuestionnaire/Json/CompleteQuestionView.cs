using System;
using System.Linq;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;


namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    public class CompleteQuestionsJsonView
    {
        public Guid PublicKey { get; set; }
        public Guid GroupPublicKey { get; set; }
        public string Title { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool Featured { get; set; }
        public bool Enabled { get; set; }
        public bool Valid { get; set; }
        public bool Answered { get; set; }
        public object Answer { get; set; }
        public bool IsInPropagatebleGroup { get; set; }
        public string Comments { get; set; }

        public CompleteQuestionsJsonView()
        {
        }

        public CompleteQuestionsJsonView(ICompleteQuestion doc, Guid groupPublicKey, bool isInPropagatebleGroup)
        {
            IsInPropagatebleGroup = isInPropagatebleGroup;
            this.GroupPublicKey = groupPublicKey;
            this.Title = doc.QuestionText;
            this.QuestionType = doc.QuestionType;
            this.PublicKey = doc.PublicKey;
            this.Featured = doc.Featured;
            this.Valid = doc.Valid;
            this.Comments = doc.Comments;
            this.Enabled = doc.Enabled;
            var answers = doc.Children.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(doc.PublicKey, a)).ToArray();
            this.Answer = doc.GetAnswerString();
            this.Answered = answers.Any(a => a.Selected == true);
        }
    }
}

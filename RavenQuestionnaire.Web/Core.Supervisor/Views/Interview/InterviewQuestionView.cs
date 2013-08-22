
using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Answer;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace Core.Supervisor.Views.Interview
{
    public class InterviewQuestionView 
    {
        public Guid PublicKey { get; set; }
        public string Text { get; set; }

        public QuestionType QuestionType { get; set; }

        public bool Mandatory { get; set; }
        public bool Featured { get; set; }
        public bool Capital { get; set; }

        public int[] PropagationVector { get; set; }

        public CompleteAnswerView[] Answers { get; set; }

        public List<InterviewQuestionComment> Comments { get; set; }
        public bool Valid { get; set; }
        public bool Enabled { get; set; }
        public bool Flagged { get; set; }

        public object Answer { get; set; }

        public InterviewQuestionView(ICompleteQuestion question, InterviewQuestion answeredQuestion)
        {
            this.PublicKey = question.PublicKey;
            this.Text = question.QuestionText;
            this.QuestionType = question.QuestionType;
            this.Mandatory = question.Mandatory;
            this.Featured = question.Featured;
            this.Capital = question.Capital;
            
            this.Answers = question.Answers.OfType<ICompleteAnswer>().Select(a => new CompleteAnswerView(question.PublicKey, a)).ToArray();

            if (answeredQuestion != null)
            {
                this.Comments = answeredQuestion.Comments;
                this.Valid = answeredQuestion.Valid;
                this.Enabled = Enabled;
                this.Flagged = Flagged;

                if (this.QuestionType == QuestionType.SingleOption || this.QuestionType == QuestionType.MultyOption)
                {
                    if (answeredQuestion.Answer != null)
                    {
                        var typedAnswers = answeredQuestion.Answer as decimal[];
                        if (typedAnswers == null)
                        {
                            decimal decimalAnswer;
                            if (!decimal.TryParse(answeredQuestion.Answer.ToString(), out decimalAnswer))
                                return;
                            typedAnswers = new decimal[] {decimalAnswer};
                        }
                        foreach (var item in this.Answers)
                        {
                            //fix this
                            foreach (var answerValue in typedAnswers)
                            {
                                if (answerValue.ToString() == item.AnswerValue)
                                {
                                    item.Selected = true;
                                    break;
                                }
                            }

                        }
                    }
                }
                else
                {
                    this.Answer = answeredQuestion.Answer;
                }
            }
        }
    }
}

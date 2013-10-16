using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;

namespace Core.Supervisor.Views.Interview
{
    [DebuggerDisplay("{Title} ({Id})")]
    public class InterviewQuestionView
    {
        public InterviewQuestionView(IQuestion question, InterviewQuestion answeredQuestion,
            Dictionary<Guid, string> variablesMap, Dictionary<string, string> answersForTitleSubstitution)
        {
            this.Id = question.PublicKey;
            this.Title = GetTitleWithSubstitutedVariables(question, answersForTitleSubstitution);
            this.QuestionType = question.QuestionType;
            this.IsMandatory = question.Mandatory;
            this.IsFeatured = question.Featured;
            this.IsCapital = question.Capital;
            this.ValidationMessage = question.ValidationMessage;
            this.ValidationExpression = this.ReplaceGuidsWithVariables(question.ValidationExpression, variablesMap);
            this.Variable = question.StataExportCaption;
            this.IsValid = true;
            this.IsEnabled = question.QuestionScope == QuestionScope.Supervisor;
            this.Scope = question.QuestionScope;

            if (question.Answers != null)
            {
                this.Options = question.Answers.Select(a => new QuestionOptionView
                {
                    Value = decimal.Parse(a.AnswerValue),
                    Label = a.AnswerText
                }).ToList();
            }

            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
            {
                this.Settings = new { numericQuestion.IsInteger };
            }

            if (answeredQuestion == null) return;

            this.IsAnswered = answeredQuestion.IsAnswered;
            this.IsEnabled = answeredQuestion.Enabled;
            this.IsFlagged = answeredQuestion.IsFlagged;
            this.Comments = answeredQuestion.Comments.Select(x => new InterviewQuestionCommentView
            {
                Id = x.Id,
                Text = x.Text,
                CommenterId = x.CommenterId,
                CommenterName = x.CommenterName,
                Date = x.Date
            }).ToList();
            this.IsValid = answeredQuestion.Valid;
            this.Answer = answeredQuestion.Answer;
        }

        private static string GetTitleWithSubstitutedVariables(IQuestion question, Dictionary<string, string> answersForTitleSubstitution)
        {
            IEnumerable<string> usedVariables = question.GetVariablesUsedInTitle();

            string title = question.QuestionText;

            foreach (string usedVariable in usedVariables)
            {
                string escapedVariable = string.Format("%{0}%", usedVariable);
                string actualAnswerOrDots = answersForTitleSubstitution.ContainsKey(usedVariable) ? answersForTitleSubstitution[usedVariable] : "[...]";

                title = title.Replace(escapedVariable, actualAnswerOrDots);
            }

            return title;
        }

        private string ReplaceGuidsWithVariables(string expression, Dictionary<Guid, string> variablesMap)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;

            string expression1 = expression;

            foreach (var pair in variablesMap)
            {
                expression1 = expression1.Replace(string.Format("[{0}]", pair.Key), string.Format("[{0}]", pair.Value));
            }

            return expression1;
        }

        public string Variable { get; set; }

        public List<QuestionOptionView> Options { get; set; }

        public QuestionScope Scope { get; set; }

        public bool IsAnswered { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public QuestionType QuestionType { get; set; }

        public bool IsMandatory { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsCapital { get; set; }
        public bool IsValid { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFlagged { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }

        public int[] PropagationVector { get; set; }

        public List<InterviewQuestionCommentView> Comments { get; set; }

        public object Answer { get; set; }

        public dynamic Settings { get; set; }
    }

    public class InterviewQuestionCommentView
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public Guid CommenterId { get; set; }
        public string CommenterName { get; set; }
    }

    public class QuestionOptionView
    {
        public object Value { get; set; }
        public string Label { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{Title} ({Id})")]
    public class InterviewQuestionView : InterviewEntityView
    {
        public InterviewQuestionView(IQuestion question, 
            InterviewQuestion answeredQuestion, 
            Dictionary<string, string> answersForTitleSubstitution, 
            bool isParentGroupDisabled, 
            decimal[] rosterVector,
            InterviewStatus interviewStatus)
        {
            this.Id = question.PublicKey;
            this.RosterVector = rosterVector;
            this.Title = GetTitleWithSubstitutedVariables(question, answersForTitleSubstitution);
            this.QuestionType = question.QuestionType;
            this.IsFeatured = question.Featured;
            this.Variable = question.StataExportCaption;
            this.IsValid = true;
            this.IsEnabled = (question.QuestionScope == QuestionScope.Supervisor) || (answeredQuestion == null) && !isParentGroupDisabled;
            this.IsReadOnly = !(question.QuestionScope == QuestionScope.Supervisor && interviewStatus < InterviewStatus.ApprovedByHeadquarters);
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
                this.Settings = new NumericQuestionSettings
                {
                    IsInteger = numericQuestion.IsInteger,
                    CountOfDecimalPlaces = numericQuestion.CountOfDecimalPlaces,
                    UseFormating = numericQuestion.UseFormatting
                };
            }

            var categoricalMultiQuestion = question as MultyOptionsQuestion;
            if (categoricalMultiQuestion != null)
            {
                this.Settings = new MultiQuestionSettings
                {
                    YesNoQuestion = categoricalMultiQuestion.YesNoView,
                    AreAnswersOrdered = categoricalMultiQuestion.AreAnswersOrdered,
                    MaxAllowedAnswers = categoricalMultiQuestion.MaxAllowedAnswers,
                    IsLinked = (categoricalMultiQuestion.LinkedToQuestionId.HasValue|| categoricalMultiQuestion.LinkedToRosterId.HasValue)
                };
            }

            var categoricalSingleQuestion = question as SingleQuestion;
            if (categoricalSingleQuestion != null)
            {
                this.Settings = new SingleQuestionSettings
                {
                    IsFilteredCombobox = categoricalSingleQuestion.IsFilteredCombobox ?? false,
                    IsCascade = categoricalSingleQuestion.CascadeFromQuestionId.HasValue,
                    IsLinked = (categoricalSingleQuestion.LinkedToQuestionId.HasValue|| categoricalSingleQuestion.LinkedToRosterId.HasValue)
                };
            }

            var textQuestion = question as TextQuestion;
            if (textQuestion != null)
            {
                this.Settings = new TextQuestionSettings
                {
                    Mask = textQuestion.Mask
                };
            }

            if (answeredQuestion == null) return;

            this.IsAnswered = answeredQuestion.IsAnswered();
            this.IsEnabled = !isParentGroupDisabled && !answeredQuestion.IsDisabled();
            this.IsFlagged = answeredQuestion.IsFlagged();
            this.Comments = (answeredQuestion.Comments?? new List<InterviewQuestionComment>()).Select(x => new InterviewQuestionCommentView
            {
                Id = x.Id,
                Text = x.Text,
                CommenterId = x.CommenterId,
                CommenterName = x.CommenterName,
                Date = x.Date
            }).ToList();
            this.Answer = answeredQuestion.Answer;

            var textListQuestion = question as ITextListQuestion;
            if (textListQuestion != null)
            {
                this.Answer = null;
                var typedAnswer = answeredQuestion.Answer as InterviewTextListAnswers;
                if (typedAnswer != null)
                {
                    this.Options = typedAnswer.Answers.Select(a => new QuestionOptionView
                    {
                        Value = a.Value,
                        Label = a.Answer
                    }).ToList();
                }
            }

            bool shouldBeValidByConvention = !this.IsEnabled;

            this.IsValid = shouldBeValidByConvention || !answeredQuestion.IsInvalid();
            this.AnswerString = this.FormatAnswerAsString(answeredQuestion.Answer);

            this.FailedValidationMessages = answeredQuestion.FailedValidationConditions.Select(x => question.ValidationConditions[x.FailedConditionIndex]).ToList();
        }

        public List<ValidationCondition> FailedValidationMessages { get; private set; }

        private string FormatAnswerAsString(object answer)
        {
            if (answer == null) return "";
            switch (this.QuestionType)
            {
                case QuestionType.SingleOption:
                    if (this.Settings != null && (this.Settings as SingleQuestionSettings).IsLinked)
                    {
                        return AnswerUtils.AnswerToString(answer);
                    }
                    else
                    {
                        return AnswerUtils.AnswerToString(answer, x => this.Options.First(o => (decimal)o.Value == x).Label);
                    }

                case QuestionType.MultyOption:
                    if ((this.Settings as MultiQuestionSettings).IsLinked)
                    {
                        return AnswerUtils.AnswerToString(answer);
                    }
                    else
                    {
                        return AnswerUtils.AnswerToString(answer, x => this.Options.First(o => (decimal)o.Value == x).Label);
                    }
                case QuestionType.DateTime:
                    if (answer is DateTime)
                    {
                        var date = (DateTime)answer;
                        return date.ToString("u");
                    }
                    break;
                case QuestionType.Numeric:
                    var intAnswer = answer as int?;
                    var longAnswer = intAnswer ??  (answer as long?);
                    var decimalAnswer = longAnswer ?? (answer as decimal?);

                    return decimalAnswer.FormatDecimal();
                case QuestionType.GpsCoordinates:
                case QuestionType.TextList:
                case QuestionType.Text:
                case QuestionType.QRBarcode:
                case QuestionType.Multimedia:
                    return AnswerUtils.AnswerToString(answer);
            }
            return "";
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

        public string AnswerString { get; private set; }

        public decimal[] RosterVector { get; set; }

        public string Variable { get; set; }

        public List<QuestionOptionView> Options { get; set; }

        public QuestionScope Scope { get; set; }

        public bool IsAnswered { get; set; }

        public string Title { get; set; }

        public QuestionType QuestionType { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsValid { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsFlagged { get; set; }

        public int[] PropagationVector { get; set; }

        public List<InterviewQuestionCommentView> Comments { get; set; }

        public object Answer { get; set; }

        public dynamic Settings { get; set; }
    }

    public class TextQuestionSettings
    {
        public string Mask { get; set; }
    }

    public class NumericQuestionSettings
    {
        public bool IsInteger { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public bool UseFormating { get; set; }
    }

    public class MultiQuestionSettings
    {
        public bool YesNoQuestion { get; set; }
        public bool AreAnswersOrdered { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public bool IsLinked { get; set; }
    }

    public class SingleQuestionSettings
    {
        public bool IsFilteredCombobox { get; set; }

        public bool IsCascade { get; set; }

        public bool IsLinked { get; set; }
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
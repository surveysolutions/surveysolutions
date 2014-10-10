using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionView : PdfEntityView
    {
        private static readonly Regex ConditionRegex = new Regex(@"\[([^\]]*)\]", RegexOptions.Compiled);

        public PdfQuestionView()
        {
            this.Answers = new List<PdfAnswerView>();
        }

        public QuestionType QuestionType { get; set; }

        public List<PdfAnswerView> Answers { get; set; }

        public string GetReadableValidationExpression()
        {
            if (string.IsNullOrWhiteSpace(this.ValidationExpression))
            {
                return null;
            }

            return this.ReplaceGuidsWithQuestionNumbers(this.ValidationExpression);
        }

        public string ValidationExpression { get; set; }

        public string GetReadableConditionExpression()
        {
            if (string.IsNullOrWhiteSpace(this.ConditionExpression))
            {
                return null;
            }

            return this.ReplaceGuidsWithQuestionNumbers(this.ConditionExpression);
        }
        
        public string ConditionExpression { get; set; }

        private string ReplaceGuidsWithQuestionNumbers(string expression)
        {
            return ConditionRegex.Replace(expression, match => {
                Guid matchId = Guid.Empty;
                if (Guid.TryParse(match.Groups[1].Value, out matchId))
                {
                    var question = GetRoot().Children.TreeToEnumerable().OfType<PdfQuestionView>().FirstOrDefault(x => x.PublicId == matchId);
                    if (question != null)
                        return "[question " + question.ItemNumber +"]";
                    else
                    {
                        return "[unknown question]";
                    }
                }

                return match.Value;
            });
        }

        public bool GetHasCondition()
        {
            return !string.IsNullOrWhiteSpace(this.ConditionExpression);
        }

        private PdfEntityView rootElement = null;

        private PdfEntityView GetRoot()
        {
            if (rootElement != null) 
                return rootElement;

            PdfEntityView currentElement = this;

            while (currentElement.GetParent() != null)
            {
                currentElement = currentElement.GetParent();
            }
            rootElement = currentElement;

            return rootElement;
        }

        private string stringItemNumber = null;

        public string GetStringItemNumber()
        {
            if (this.stringItemNumber == null)
            {
                this.stringItemNumber = string.Join(".", this.GetQuestionNumberSections().Select(x => x.ToString(CultureInfo.InvariantCulture).PadLeft(5, '0')));
            }

            return this.stringItemNumber;
        }
    }
}
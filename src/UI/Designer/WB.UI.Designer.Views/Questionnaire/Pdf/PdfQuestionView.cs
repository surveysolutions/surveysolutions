using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionView : PdfEntityView
    {
        private static Regex conditionRegex = new Regex(@"\[([^\]]*)\]", RegexOptions.Compiled);

        public PdfQuestionView()
        {
            this.Answers = new List<PdfAnswerView>();
        }

        public QuestionType QuestionType { get; set; }

        public List<PdfAnswerView> Answers { get; set; }

        public string GetReadableValidationExpression()
        {
            if (this.ValidationExpression == null)
            {
                return null;
            }

            return this.ReplaceGuidsWithQuestionNumbers(this.ValidationExpression);
        }

        public string ValidationExpression { get; set; }

        public string GetReadableConditionExpression()
        {
            if (string.IsNullOrEmpty(this.ConditionExpression))
            {
                return null;
            }

            return this.ReplaceGuidsWithQuestionNumbers(this.ConditionExpression);
        }

        public string ConditionExpression { get; set; }

        private string ReplaceGuidsWithQuestionNumbers(string expression)
        {
            return conditionRegex.Replace(expression, match => {
                Guid matchId = Guid.Empty;
                if (Guid.TryParse(match.Groups[1].Value, out matchId))
                {
                    var question = GetRoot().Children.TreeToEnumerable().OfType<PdfQuestionView>().FirstOrDefault(x => x.PublicId == matchId);
                    return "[question " + question.ItemNumber +"]";
                }

                return match.Value;
            });
        }

        public string Variable { get; set; }

        public bool GetHasCondition()
        {
            return !string.IsNullOrEmpty(this.GetReadableConditionExpression());
        }

        private PdfEntityView GetRoot()
        {
            var next = this.GetParent();
            do
            {
                if (next.GetParent() != null)
                {
                    next = next.GetParent();
                }
                else
                {
                    return next;
                }
            } while (next != null);

            return next;
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
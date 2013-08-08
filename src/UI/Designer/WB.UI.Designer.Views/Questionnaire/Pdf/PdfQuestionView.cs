using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionView : PdfEntityView
    {
        private static Regex conditionRegex = new Regex(@"\[([^\]]*)\]", RegexOptions.Compiled);

        private string _condition;
        private string _validationExpression;

        public PdfQuestionView()
        {
            this.Answers = new List<PdfAnswerView>();
        }

        public QuestionType QuestionType { get; set; }

        public List<PdfAnswerView> Answers { get; set; }

        public string ValidationExpression
        {
            get
            {
                if (_validationExpression == null)
                {
                    return null;
                }

                return ReplaceGuidsWithQuestionNumbers(_validationExpression);
            }
            set { _validationExpression = value; }
        }

        public string Condition
        {
            get
            {
                if (string.IsNullOrEmpty(_condition))
                {
                    return null;
                }

                return ReplaceGuidsWithQuestionNumbers(_condition);
            }
            set
            {
                _condition = value;
            }
        }

        private string ReplaceGuidsWithQuestionNumbers(string expression)
        {
            return conditionRegex.Replace(expression, match => {
                Guid matchId = Guid.Empty;
                if (Guid.TryParse(match.Groups[1].Value, out matchId))
                {
                    var question = Root.Children.TreeToEnumerable().OfType<PdfQuestionView>().FirstOrDefault(x => x.Id == matchId);
                    return "[question " + question.ItemNumber +"]";
                }

                return match.Value;
            });
        }

        public string Variable { get; set; }

        public bool HasCodition
        {
            get
            {
                return !string.IsNullOrEmpty(Condition);
            }
        }

        private PdfEntityView Root
        {
            get
            {
                var next = Parent;
                do
                {
                    if (next.Parent != null)
                    {
                        next = next.Parent;
                    }
                    else
                    {
                        return next;
                    }
                } while (next != null);

                return next;
            }
        }
    }
}
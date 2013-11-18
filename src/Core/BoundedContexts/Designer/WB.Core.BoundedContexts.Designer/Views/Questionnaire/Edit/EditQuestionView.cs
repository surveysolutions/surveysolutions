using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class CategoricalSettings
    {
        public Guid? LinkedToQuestionId { get; set; }
        public EditAnswerView[] Answers { get; set; }
    }

    public class SingleOptionSettings : CategoricalSettings { }

    public class MultyOptionSettings : CategoricalSettings
    {
        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
    }
    public class NumericSettings
    {
        public bool IsInteger { get; set; }
        public int? MaxValue { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
    }

    public class EditQuestionView : ICompositeView
    {
        public EditQuestionView(IQuestion doc, Guid? parentId)
        {
            this.Id = doc.PublicKey;
            this.ParentId = parentId;
            this.Title = doc.QuestionText.Replace(Environment.NewLine, " ");
            this.QuestionType = doc.QuestionType;
            this.QuestionScope = doc.QuestionScope;
            this.ConditionExpression = doc.ConditionExpression;
            this.ValidationExpression = doc.ValidationExpression;
            this.ValidationMessage = doc.ValidationMessage;
            this.Alias = doc.StataExportCaption;
            this.Instructions = doc.Instructions;
            this.Featured = doc.Featured;
            this.Mandatory = doc.Mandatory;
            this.Capital = doc.Capital;

            if (doc.QuestionType == QuestionType.SingleOption)
            {
                this.Settings = new SingleOptionSettings
                {
                    LinkedToQuestionId = doc.LinkedToQuestionId,
                    Answers = doc.Answers.Select(a => new EditAnswerView(a)).ToArray()
                };
            }

            var numericQuestion = doc as INumericQuestion;
            if (numericQuestion != null)
            {
                this.Settings = new NumericSettings
                    {
                        MaxValue = numericQuestion.MaxValue,
                        IsInteger = numericQuestion.IsInteger,
                        CountOfDecimalPlaces = numericQuestion.CountOfDecimalPlaces
                    };
            }

            var multyoptionQuestion = doc as IMultyOptionsQuestion;
            if (multyoptionQuestion != null)
            {
                this.Settings = new MultyOptionSettings
                    {
                        LinkedToQuestionId = doc.LinkedToQuestionId,
                        Answers = doc.Answers.Select(a => new EditAnswerView(a)).ToArray(),
                        AreAnswersOrdered = multyoptionQuestion.AreAnswersOrdered,
                        MaxAllowedAnswers = multyoptionQuestion.MaxAllowedAnswers
                    };
            }
        }

        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string ConditionExpression { get; set; }

        public bool Featured { get; set; }

        public string Instructions { get; set; }

        public bool Mandatory { get; set; }

        public bool Capital { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public QuestionType QuestionType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public QuestionScope QuestionScope { get; set; }

        public string Alias { get; set; }

        public string Title { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; set; }

        public dynamic Settings { get; set; }
    }
}
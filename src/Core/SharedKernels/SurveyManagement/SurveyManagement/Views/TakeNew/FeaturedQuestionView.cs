using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TakeNew
{
    public class FeaturedQuestionView 
    {
        public FeaturedQuestionView(IQuestion doc, Guid? parentId)
        {
            this.Id = doc.PublicKey;
            this.ParentId = parentId;
            this.Title = doc.QuestionText.Replace(System.Environment.NewLine, " ");
            this.QuestionType = doc.QuestionType;
            this.QuestionScope = doc.QuestionScope;
            this.ConditionExpression = doc.ConditionExpression;
            this.ValidationExpression = doc.ValidationExpression;
            this.ValidationMessage = doc.ValidationMessage;
            this.StataExportCaption = doc.StataExportCaption;
            this.Instructions = doc.Instructions;
            this.AnswerOrder = doc.AnswerOrder;
            this.Featured = doc.Featured;
            this.Mandatory = doc.Mandatory;
            this.Capital = doc.Capital;
            this.LinkedToQuestionId = doc.LinkedToQuestionId;
            this.Answers = null;
            this.Triggers = null;

            this.Answers = doc.Answers.Select(a => new FeaturedAnswerView(a)).ToArray();

            var numericQuestion = doc as INumericQuestion;
            if (numericQuestion != null)
            {
                this.Settings = new
                {
                    numericQuestion.IsInteger,
                    numericQuestion.CountOfDecimalPlaces
                };
            }

            var multyoptionQuestion = doc as IMultyOptionsQuestion;
            if (multyoptionQuestion != null)
            {
                this.Settings = new
                {
                    AreAnswersOrdered = multyoptionQuestion.AreAnswersOrdered,
                    multyoptionQuestion.MaxAllowedAnswers
                };
            }

            var singleoptionQuestion = doc as SingleQuestion;
            if (singleoptionQuestion != null)
            {
                this.Settings = new
                {
                    IsFilteredCombobox = singleoptionQuestion.IsFilteredCombobox
                };
            }

            var textQuestion = doc as TextQuestion;
            if (textQuestion != null)
            {
                this.Settings = new
                {
                    Mask = textQuestion.Mask
                };
            }
        }

        public Guid? ParentId { get; set; }

        public FeaturedAnswerView[] Answers { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Order AnswerOrder { get; set; }

        public string ConditionExpression { get; set; }

        public bool Featured { get; set; }

        public string Instructions { get; set; }

        public bool Mandatory { get; set; }

        public bool Capital { get; set; }

        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public QuestionType QuestionType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public QuestionScope QuestionScope { get; set; }

        public string StataExportCaption { get; set; }

        public string Title { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; set; }

        public List<Guid> Triggers { get; set; }

        public int MaxValue { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public dynamic Settings { get; set; }
    }
}
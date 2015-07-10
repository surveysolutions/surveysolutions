using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    internal class QuestionDataParser : IQuestionDataParser
    {
        public ValueParsingResult TryParse(string answer, IQuestion question, QuestionnaireDocument questionnaire, out KeyValuePair<Guid, object> parsedValue)
        {
            parsedValue = new KeyValuePair<Guid, object>();

            if (string.IsNullOrEmpty(answer))
                return ValueParsingResult.ValueIsNullOrEmpty;

            if (question == null)
                return ValueParsingResult.QuestionWasNotFound;

            if (question.LinkedToQuestionId.HasValue)
                return ValueParsingResult.UnsupportedLinkedQuestion;

            if (question is IMultimediaQuestion)
                return ValueParsingResult.UnsupportedMultimediaQuestion;

            switch (question.QuestionType)
            {
                case QuestionType.Text:
                    var textQuestion = (TextQuestion)question;
                     parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, answer);
                    if (!string.IsNullOrEmpty(textQuestion.Mask))
                    {
                        var formatter = new MaskedFormatter(textQuestion.Mask);
                        bool maskMatches = formatter.IsTextMaskMatched(answer);
                        if (!maskMatches)
                        {
                            return ValueParsingResult.ParsedValueIsNotAllowed;
                        }
                    }
                   
                    return ValueParsingResult.OK;
                case QuestionType.QRBarcode:
                case QuestionType.TextList:
                    parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, answer);
                    return ValueParsingResult.OK;
                case QuestionType.GpsCoordinates:
                    var parsedAnswer = GeoPosition.Parse(answer);
                    if (parsedAnswer == null)
                        return ValueParsingResult.AnswerAsGpsWasNotParsed;

                    parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, parsedAnswer);
                    return ValueParsingResult.OK;
                
                case QuestionType.AutoPropagate:
                    int intValue;
                    if (!int.TryParse(answer, out intValue))
                        return ValueParsingResult.AnswerAsIntWasNotParsed;

                    parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, intValue);
                    return ValueParsingResult.OK;
                                        
                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;
                    
                    // please don't trust R# warning below. if you simplify expression with '?' then answer would be saved as decimal even for integer question
                    if (numericQuestion.IsInteger)
                    {
                        int intNumericValue;
                        if (!int.TryParse(answer, out intNumericValue))
                            return ValueParsingResult.AnswerAsIntWasNotParsed;

                        parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, intNumericValue);

                        if (intNumericValue < 0 &&
                            questionnaire.FirstOrDefault<IGroup>(group => group.RosterSizeQuestionId == question.PublicKey) != null)
                            return ValueParsingResult.AnswerIsIncorrectBecauseQuestionIsUsedAsSizeOfRosterGroupAndSpecifiedAnswerIsNegative;

                        return ValueParsingResult.OK;
                    }
                    else
                    {
                        decimal decimalNumericValue;
                        if (!decimal.TryParse(answer, out decimalNumericValue))
                            return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                        {
                            parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, decimalNumericValue);

                            return ValueParsingResult.OK;
                        }
                    }

                case QuestionType.DateTime:
                    DateTime date;
                    if (!DateTime.TryParse(answer, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out date))
                        return ValueParsingResult.AnswerAsDateTimeWasNotParsed;
                    parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, date);
                    return ValueParsingResult.OK;

                case QuestionType.SingleOption:
                    var singleOption = question as SingleQuestion;
                    if (singleOption == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;

                    decimal decimalAnswerValue;
                    if (!decimal.TryParse(answer, out decimalAnswerValue))
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                    if (!this.GetAnswerOptionsAsValues(question).Contains(decimalAnswerValue))
                        return ValueParsingResult.ParsedValueIsNotAllowed;

                    parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, decimalAnswerValue);
                    return ValueParsingResult.OK;

                case QuestionType.MultyOption:
                    var multyOption = question as MultyOptionsQuestion;
                    if (multyOption == null)
                        return ValueParsingResult.QuestionTypeIsIncorrect;
                    
                    decimal answerValue;
                    if (!decimal.TryParse(answer, out answerValue))
                        return ValueParsingResult.AnswerAsDecimalWasNotParsed;
                    if (!this.GetAnswerOptionsAsValues(question).Contains(answerValue))
                        return ValueParsingResult.ParsedValueIsNotAllowed;
                    parsedValue = new KeyValuePair<Guid, object>(question.PublicKey, answerValue);
                    return ValueParsingResult.OK;                    
            }

            return ValueParsingResult.GeneralErrorOccured;                    

        }

        public KeyValuePair<Guid, object>? BuildAnswerFromStringArray(string[] answers, IQuestion question, QuestionnaireDocument questionnaire)
        {
            if (question == null)
                return null;

            var typedAnswers = new List<object>();

            foreach (var answer in answers)
            {
                KeyValuePair<Guid, object> parsedAnswer;
                if(this.TryParse(answer, question, questionnaire, out parsedAnswer) != ValueParsingResult.OK)
                    continue;
                
                typedAnswers.Add(parsedAnswer.Value);
            }

            if (typedAnswers.Count == 0)
                return null;
            
            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    return new KeyValuePair<Guid, object>(question.PublicKey, typedAnswers.Select(a => (decimal)a).ToArray());
                case QuestionType.TextList:
                    return new KeyValuePair<Guid, object>(question.PublicKey,
                        typedAnswers.Select((a, i) => new Tuple<decimal, string>(i + 1, (string)a)).ToArray());
                default:
                    return new KeyValuePair<Guid, object>(question.PublicKey, typedAnswers.First());
            }
        }

        
        private decimal[] GetAnswerOptionsAsValues(IQuestion question)
        {
            return
                question.Answers.Select(answer => this.ParseAnswerOptionValueOrThrow(answer.AnswerValue))
                    .Where(o => o.HasValue)
                    .Select(o => o.Value)
                    .ToArray();
        }

        private decimal? ParseAnswerOptionValueOrThrow(string value)
        {
            decimal parsedValue;

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue))
                return null;

            return parsedValue;
        }
    }
}
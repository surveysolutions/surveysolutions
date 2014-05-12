namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects
{
    public enum ValueParsingResult
    {
        OK,
        QuestionWasNotFound,
        ValueIsNullOrEmpty,
        
        AnswerAsIntWasNotParsed,
        AnswerAsDecimalWasNotParsed,
        AnswerAsDateTimeWasNotParsed,
        AnswerAsGpsWasNotParsed,
        ParsedValueIsNotAllowed,
        QuestionTypeIsIncorrect,
        UnsupportedLinkedQuestion,

        GeneralErrorOccured
    }
}

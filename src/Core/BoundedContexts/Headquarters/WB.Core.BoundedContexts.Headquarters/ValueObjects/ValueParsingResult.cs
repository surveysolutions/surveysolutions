namespace WB.Core.BoundedContexts.Headquarters.ValueObjects
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

        GeneralErrorOccured,
        UnsupportedMultimediaQuestion,
        CommaIsUnsupportedInAnswer
    }
}

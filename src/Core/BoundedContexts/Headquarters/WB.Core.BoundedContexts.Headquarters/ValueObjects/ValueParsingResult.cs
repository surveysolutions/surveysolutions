namespace WB.Core.BoundedContexts.Headquarters.ValueObjects
{
    public enum ValueParsingResult
    {
        OK = 0,
        QuestionWasNotFound = 1,
        ValueIsNullOrEmpty = 2,
        
        AnswerAsIntWasNotParsed = 3,
        AnswerAsDecimalWasNotParsed = 4,
        AnswerAsDateTimeWasNotParsed = 5,
        AnswerAsGpsWasNotParsed = 6 ,
        ParsedValueIsNotAllowed = 7 ,
        QuestionTypeIsIncorrect = 8,
        UnsupportedLinkedQuestion = 9,

        GeneralErrorOccured = 10,
        UnsupportedMultimediaQuestion = 11,
        CommaIsUnsupportedInAnswer = 12 ,
        UnsupportedAreaQuestion = 13,
        UnsupportedAudioQuestion = 14
    }
}

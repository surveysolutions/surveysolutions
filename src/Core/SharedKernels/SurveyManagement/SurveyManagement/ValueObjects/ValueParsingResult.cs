namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects
{
    public enum ValueParsingResult
    {
        OK,
        QuestionWasNotFound,
        ValueIsNullOrEmpty,
        
        AnswerAsIntWasNotParsed,
        AnswerIsIncorrectBecauseQuestionIsUsedAsSizeOfRosterGroupAndSpecifiedAnswerIsNegative,
        AnswerIsIncorrectBecauseQuestionIsUsedAsSizeOfRosterGroupAndSpecifiedAnswerIsMoreThan40,
        AnswerAsDecimalWasNotParsed,
        AnswerAsDateTimeWasNotParsed,
        AnswerAsGpsWasNotParsed,
        ParsedValueIsNotAllowed,
        QuestionTypeIsIncorrect,
        UnsupportedLinkedQuestion,

        GeneralErrorOccured,
        UnsupportedMultimediaQuestion
    }
}

namespace WB.Core.BoundedContexts.Designer.ValueObjects
{
    public enum QuestionnaireVerificationReferenceType
    {
        Question = 1,
        Group = 10,
        Roster = 20,
        StaticText = 30,
        Macro = 40,
        LookupTable = 50,
        Attachment = 60,
        Variable = 70,
        Translation = 80,
        Questionnaire = 90
    }

    public enum QuestionnaireVerificationReferenceProperty
    {
        None = 0,
        Title = 1,
        ValidationExpression = 2,
        ValidationMessage = 3,
        EnablingCondition = 4,
        VariableName = 5,
        Option = 6,
        VariableContent = 7,
        FixedRosterItem = 8,
        OptionsFilter = 9,
        AttachmentName = 10,
        VariableLabel = 11
    }
}

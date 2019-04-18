namespace WB.Services.Export.Questionnaire
{
    public enum QuestionSubtype
    {
        Unknown = 0,
        MultiOptionYesNo = 1,
        MultiOptionLinkedFirstLevel = 2,
        MultiOptionOrdered = 3,
        MultiOptionYesNoOrdered = 4,
        DateTimeTimestamp = 5,
        SingleOptionLinkedFirstLevel = 6,
        MultiOptionLinkedNestedLevel = 7,
        SingleOptionLinkedNestedLevel = 8,
        NumericInteger = 9,
        MultyOption_Combobox = 10
    }
}

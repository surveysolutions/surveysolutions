namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects
{
    public enum AnswerType
    {
        Integer = 1,
        Decimal = 2,
        DateTime = 3,
        
        OptionCodeArray = 4,
        RosterVectorArray = 5,

        OptionCode = 6,
        RosterVector = 7,
       
        DecimalAndStringArray = 8,
        String = 9,
        GpsData = 10,
        FileName = 11,

        YesNoArray = 12,
    }
}
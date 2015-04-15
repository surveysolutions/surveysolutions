namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public enum QuestionModelType
    {
        SingleOption = 1,
        LinkedSingleOption,
        MultiOption,
        LinkedMultiOption,
        IntegerNumeric,
        RealNumeric,
        MaskedText,
        TextList,
        QrBarcode,
        Multimedia,
        DateTime,
        GpsCoordinates,
    }
}
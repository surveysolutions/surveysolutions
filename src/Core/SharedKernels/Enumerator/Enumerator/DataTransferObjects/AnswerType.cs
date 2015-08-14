namespace WB.Core.SharedKernels.Enumerator.DataTransferObjects
{
    public enum AnswerType
    {
        IntegerNumericAnswer = 1,
        RealNumericAnswer = 2,
        DateTimeAnswer = 3,
        
        MultiOptionAnswer = 4,
        LinkedMultiOptionAnswer = 5,

        SingleOptionAnswer = 6,
        LinkedSingleOptionAnswer = 7,
       
        TextListAnswer = 8,
        TextAnswer = 9,
        GpsAnswer= 10,
        MultimediaAnswer = 11,
        QRBarcodeAnswer = 12
    }
}
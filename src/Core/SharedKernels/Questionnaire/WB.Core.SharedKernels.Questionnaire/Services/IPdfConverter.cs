namespace WB.Core.SharedKernels.Questionnaire.Services
{
    public interface IPdfConverter
    {
        byte[] CreateThumbnail(byte[] pdfBytes);
    }
}

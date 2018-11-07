namespace WB.Core.SharedKernels.Questionnaire.Services
{
    public interface IVideoConverter
    {
        byte[] CreateThumbnail(string pathToVideo);
        byte[] CreateThumbnail(byte[] videoBytes);
    }
}

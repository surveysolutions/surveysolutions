namespace WB.UI.Shared.Web.Services
{
    public interface IImageProcessingService
    {
        void ValidateImage(byte[] source);
        byte[] ResizeImage(byte[] source, int? height = null, int? width = null);
    }
}

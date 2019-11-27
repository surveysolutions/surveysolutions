namespace WB.UI.Shared.Web.Services
{
    public interface IImageProcessingService
    {
        void Validate(byte[] source);
        byte[] ResizeImage(byte[] source, int height, int width);
    }
}

namespace WB.UI.Shared.Web.Services
{
    public interface IImageProcessingService
    {
        byte[] ResizeImage(byte[] source, int height, int width);
    }
}

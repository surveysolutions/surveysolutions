namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IQRCodeHelper
    {
        string GetQRCodeAsBase64StringSrc(string relativeUrl, int height, int width, int margin = 0);
        bool SupportQRCodeGeneration();

        string GetFullUrl(string relativeUrl);

        string GetBaseUrl();
    }
}

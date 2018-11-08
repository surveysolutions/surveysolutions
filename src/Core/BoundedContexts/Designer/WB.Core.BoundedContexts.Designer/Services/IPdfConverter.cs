namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IPdfConverter
    {
        byte[] CreateThumbnail(byte[] pdfBytes);
    }
}

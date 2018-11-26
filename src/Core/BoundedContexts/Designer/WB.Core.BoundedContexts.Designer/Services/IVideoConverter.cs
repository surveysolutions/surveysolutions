namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IVideoConverter
    {
        byte[] CreateThumbnail(byte[] videoBytes);
    }
}

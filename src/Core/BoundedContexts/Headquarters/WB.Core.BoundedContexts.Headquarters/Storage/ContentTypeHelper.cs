using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.Storage;

public static class ContentTypeHelper
{
    public static string GetImageContentType(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return null;

        var extension = Path.GetExtension(filename).ToLower();
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".gif":
                return "image/gif";
            case ".bmp":
                return "image/bmp";
            case ".webp":
                return "image/webp";
            case ".tiff":
            case ".tif":
                return "image/tiff";
            default:
                return "image/jpeg";
        }
    }

    public static string GetAudioContentType(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return null;
        
        var extension = Path.GetExtension(filename).ToLower();
        switch (extension)
        {
            case ".aac":
                return "audio/aac";
            case ".m4a":
                return "audio/mp4";
            case ".mp3":
                return "audio/mpeg";
            case ".wav":
                return "audio/wav";
            case ".ogg":
                return "audio/ogg";
            case ".flac":
                return "audio/flac";
            default:
                return "audio/mp4";
        }
    }
}

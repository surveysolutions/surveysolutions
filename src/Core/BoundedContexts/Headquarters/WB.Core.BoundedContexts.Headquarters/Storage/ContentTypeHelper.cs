using System.IO;
using Microsoft.AspNetCore.StaticFiles;

namespace WB.Core.BoundedContexts.Headquarters.Storage;

public static class ContentTypeHelper
{
    public static string GetImageContentType(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return null;

        var extension = Path.GetExtension(filename).ToLower();
        var provider = new FileExtensionContentTypeProvider();
        if (provider.TryGetContentType(extension, out var contentType))
            return contentType;
        
        return "image/jpeg";
    }

    public static string GetAudioContentType(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return null;
        
        var extension = Path.GetExtension(filename).ToLower();
        var provider = new FileExtensionContentTypeProvider();
        if (provider.TryGetContentType(extension, out var contentType))
            return contentType;

        return "audio/mp4";
    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Path = System.IO.Path;

namespace WB.UI.Designer.Areas.Pdf.Controllers;

public class PdfCssUrlRewriteMiddleware
{
    private readonly RequestDelegate next;
    private readonly IWebHostEnvironment hostingEnvironment;

    public PdfCssUrlRewriteMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnvironment)
    {
        this.next = next;
        this.hostingEnvironment = hostingEnvironment;
    }

    public Task Invoke(HttpContext context)
    {
        if (context.Request.Path.HasValue)
        {
            var pathValue = context.Request.Path.Value;
            var pdfCssMatched = (pathValue?.StartsWith("/css/pdf", StringComparison.Ordinal) ?? false)
                                 && pathValue.EndsWith(".css", StringComparison.Ordinal);

            if (pdfCssMatched)
                context.Request.Path = PdfCssFileUrl;
        }
        
        return next.Invoke(context);
    }

    private PathString? pdfCssFileUrl;
    private PathString PdfCssFileUrl
    {
        get { return pdfCssFileUrl ??= FindPdfCssFile(); }
    }

    private PathString FindPdfCssFile()
    {
        var rootPath = hostingEnvironment.WebRootPath;
        var cssFolder = Path.Combine(rootPath, "css");
        var files = Directory.GetFiles(cssFolder, "pdf*.css", SearchOption.TopDirectoryOnly);
        if (files.Length != 1)
            throw new ArgumentException("Can't find pdf css file");
        var pdfCssFileName = Path.GetFileName(files[0]);
        return new PathString("/css/" + pdfCssFileName);
    }
}
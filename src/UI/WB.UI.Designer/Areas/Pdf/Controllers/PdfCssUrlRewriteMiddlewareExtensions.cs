using System;
using Microsoft.AspNetCore.Builder;

namespace WB.UI.Designer.Areas.Pdf.Controllers;

public static class PdfCssUrlRewriteMiddlewareExtensions
{
    public static IApplicationBuilder UsePdfCssResolver(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<PdfCssUrlRewriteMiddleware>();
    }
}
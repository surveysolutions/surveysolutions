using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace WB.UI.Designer.Areas.Pdf.Services;

public class PdfRender : IPdfRender, IAsyncDisposable
{
    private IPlaywright? playwright;
    private IBrowser? browser;
    private readonly SemaphoreSlim browserInitLock = new(1, 1);
    private bool disposed;

    public async Task<byte[]> RenderPdf(string questionnaireHtml, string footerHtml, CancellationToken token)
    {
        IBrowserContext? context = null;
        IPage? page = null;
        try
        {
            var browserInstance = await GetBrowserAsync(token).ConfigureAwait(false);
            context = await browserInstance.NewContextAsync().WaitAsync(token).ConfigureAwait(false);
            page = await context.NewPageAsync().WaitAsync(token).ConfigureAwait(false);
            await page.RouteAsync("**/*.js", async route => await route.AbortAsync()).WaitAsync(token).ConfigureAwait(false);
            await page.SetContentAsync(questionnaireHtml, new PageSetContentOptions
            {
                Timeout = 120_000,
                WaitUntil = WaitUntilState.DOMContentLoaded,
            }).WaitAsync(token).ConfigureAwait(false);

            var content = await page.PdfAsync(new PagePdfOptions()
            {
                HeaderTemplate = "<html></html>",
                FooterTemplate = footerHtml,
                Format = "A4",
                DisplayHeaderFooter = true,
            }).WaitAsync(token).ConfigureAwait(false);
            
            return content;
        }
        finally
        {
            if (page != null)
            {
                try { await page.CloseAsync().ConfigureAwait(false); } catch { /* ignore */ }
            }
            if (context != null)
            {
                try { await context.CloseAsync().ConfigureAwait(false); } catch { /* ignore */ }
            }
        }
    }
    
    private async Task<IBrowser> GetBrowserAsync(CancellationToken token)
    {
        if (browser != null) 
            return browser;
        
        await browserInitLock.WaitAsync(token).ConfigureAwait(false);
        
        try
        {
            if (browser == null)
            {
                playwright = await Playwright.CreateAsync().WaitAsync(token).ConfigureAwait(false);
                browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                        "--disable-gpu",
                        "--no-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-extensions",
                        "--disable-javascript"
                    }
                }).WaitAsync(token).ConfigureAwait(false);
            }
            return browser;
        }
        finally
        {
            browserInitLock.Release();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (disposed) 
            return;
        disposed = true;
        
        if (browser != null)
        {
            try { await browser.CloseAsync(); } catch { /* ignore */ }
            browser = null;
        }
        
        playwright?.Dispose();
        browserInitLock.Dispose();
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Vite.Extensions.AspNetCore;

/// <remarks>See Github sources from <see href="https://github.com/alfeg/vite-aspnetcore">Source</see></remarks>
public class ViteTagHelperComponent : TagHelperComponent
{
    private readonly IWebHostEnvironment _webHost;
    private readonly IOptions<ViteTagOptions> _options;
    private readonly IMemoryCache _memoryCache;

    public ViteTagHelperComponent(IWebHostEnvironment webHost,
        IOptions<ViteTagOptions> options,
        IMemoryCache memoryCache)
    {
        _webHost = webHost;
        _options = options;
        _memoryCache = memoryCache;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (context.TagName.Equals("head", StringComparison.OrdinalIgnoreCase))
        {
            output.PreContent
                .AppendHtml(await RenderViteHtmlAsync(ViteRender.Css).ConfigureAwait(false));
        }
        else if (context.TagName.Equals("body", StringComparison.OrdinalIgnoreCase))
        {
            output.PreContent
                .AppendHtml(await RenderViteHtmlAsync(ViteRender.Js).ConfigureAwait(false));
        }
    }

    enum ViteRender
    {
        Css, Js
    }

    private async ValueTask<IHtmlContent> RenderViteHtmlAsync(ViteRender renderPart)
    {
        var result = await _memoryCache.GetOrCreateAsync("fs://assets/" + renderPart, async entry =>
        {
            entry.AddExpirationToken(_webHost.WebRootFileProvider.Watch("assets/.vite/manifest.json"));
            return await RenderPart(renderPart).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return new HtmlString(result ?? string.Empty);
    }

    private async Task<string> RenderPart(ViteRender renderPart)
    {
        var manifestJson = _webHost.WebRootFileProvider.GetFileInfo("assets/.vite/manifest.json");
        if (!manifestJson.Exists)
        {
            if (renderPart == ViteRender.Js)
            {
                return $@"<script type='module' src='/.vite/@vite/client'></script><script type='module' src='/.vite/{_options.Value.Entry}'></script>";
            }

            return string.Empty;
        }

        await using var manifestStream = manifestJson.CreateReadStream();
        var manifest = await JsonSerializer.DeserializeAsync<Dictionary<string, ViteManifestItem?>>(manifestStream)
                           .ConfigureAwait(false)
                       ?? new Dictionary<string, ViteManifestItem?>();

        var entryItem = manifest.FirstOrDefault(v => v.Value?.isEntry == true);

        IHtmlContentBuilder html = new HtmlContentBuilder();

        if (renderPart == ViteRender.Css)
        {
            foreach (string css in entryItem.Value?.css ?? Array.Empty<string>())
            {
                var tag = new TagBuilder("link");
                tag.Attributes.Add("rel", "stylesheet");
                tag.Attributes.Add("href", $"/assets/{css}");
                tag.Attributes.Add("type", "text/css");
                tag.TagRenderMode = TagRenderMode.SelfClosing;
                html = html.AppendHtml(tag);
            }
        }
        else if (renderPart == ViteRender.Js)
        {
            var tag = new TagBuilder("script");
            tag.Attributes.Add("src", $"/assets/{entryItem.Value?.file}");
            tag.Attributes.Add("type", "module");
            tag.TagRenderMode = TagRenderMode.Normal;
            html = html.AppendHtml(tag);
        }

        var sb = new StringBuilder();
        await using var sw = new StringWriter(sb);
        html.WriteTo(sw, HtmlEncoder.Default);
        
        return sb.ToString();
    }
}

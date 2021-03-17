using System;
using System.IO;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace Main.Core.Entities.SubEntities
{
    public static class QuestionnaireMarkdown
    {
        private static readonly Lazy<MarkdownPipeline> Pipeline = new Lazy<MarkdownPipeline>(() =>
        {
            var builder = new MarkdownPipelineBuilder();

            builder.BlockParsers.Clear();
            builder.InlineParsers.Clear();

            builder.BlockParsers.AddIfNotAlready<HtmlBlockParser>();
            builder.BlockParsers.AddIfNotAlready<ParagraphBlockParser>();
            builder.InlineParsers.AddIfNotAlready<HtmlEntityParser>();
            builder.InlineParsers.AddIfNotAlready<LinkInlineParser>();
            builder.InlineParsers.AddIfNotAlready<AutolinkInlineParser>();

            var pipeline = builder.Build();
            return pipeline;
        });


        public static string ToHtml(string text)
        {
            using (var writer = new StringWriter())
            {
                var renderer = new HtmlRenderer(writer) { EnableHtmlForBlock = false };

                Pipeline.Value.Setup(renderer);
                var document = Markdown.Parse(text, Pipeline.Value);
                renderer.Render(document);
                var sb = writer.GetStringBuilder();
                if (sb.Length > 0 && sb[sb.Length - 1] == '\n')
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }
    }
}

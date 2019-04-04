using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using Markdig;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("{ToString()}")]
    public class SubstitutionText
    {
        private readonly Identity identity;
        private readonly ISubstitutionService substitutionService;
        private readonly IVariableToUIStringService variableToUiStringService;
        private readonly List<SubstitutionVariable> substitutionVariables;
        private InterviewTree tree;

        public SubstitutionText()
        {
        }

        public SubstitutionText(
            Identity identity,
            string text,
            List<SubstitutionVariable> variables,
            ISubstitutionService substitutionService,
            IVariableToUIStringService variableToUiStringService)
        {
            var markdownReplacedText = string.IsNullOrEmpty(text) ? text : MarkdownTextToHtml(text);

            this.Text = markdownReplacedText;
            this.BrowserReadyText = markdownReplacedText;
            this.originalText = markdownReplacedText;
            this.identity = identity;
            this.substitutionService = substitutionService;
            this.variableToUiStringService = variableToUiStringService;
            this.substitutionVariables = variables;
        }

        public void SetTree(InterviewTree interviewTree)
        {
            this.tree = interviewTree;
        }

        private readonly string originalText;
        public string Text { get; private set; }
        public string BrowserReadyText { get; private set; }

        public bool HasSubstitutions => this.substitutionVariables?.Count > 0;
        
        public void ReplaceSubstitutions()
        {
            if (!this.HasSubstitutions)
                return;

            this.Text = GetTextWithReplacedSubstitutions(false);
            this.BrowserReadyText = GetTextWithReplacedSubstitutions(shouldAddBrowserTags: true);
        }

        private string GetTextWithReplacedSubstitutions(bool shouldAddBrowserTags)
        {
            var textWithReplacedSubstitutions = this.originalText;

            foreach (var substitution in this.substitutionVariables)
            {
                string substitutionResult = null;
                bool shouldEncode = true;

                var treeEntity = this.tree.FindEntityInQuestionBranch(substitution.Id, this.identity);
                if(treeEntity != null && !treeEntity.IsDisabled())
                {
                    switch (treeEntity)
                    {
                        case InterviewTreeRoster roster:
                            substitutionResult = roster.RosterTitle;
                            break;

                        case InterviewTreeVariable variable:
                            substitutionResult = this.variableToUiStringService.VariableToUIString(variable.Value);
                            break;


                        case InterviewTreeQuestion question:
                            string answerString = question.GetAnswerAsString(CultureInfo.CurrentCulture);
                            shouldEncode = false;

                            if (shouldAddBrowserTags && question.IsDateTime && question.IsAnswered())
                            {
                                var asDateTime = question.GetAsInterviewTreeDateTimeQuestion();
                                var dateTime = asDateTime.GetAnswer().Value;

                                substitutionResult = asDateTime.IsTimestamp 
                                    ? $"<time datetime=\"{dateTime:O}\">{dateTime.ToString(asDateTime.UiFormatString)}</time>" 
                                    : $"<time date=\"{dateTime:yyyy-MM-dd}\">{dateTime.ToString(asDateTime.UiFormatString)}</time>";
                        }
                        else
                        {
                            substitutionResult = shouldAddBrowserTags ? WebUtility.HtmlEncode(answerString ?? string.Empty) : answerString;
                        }
                        
                        break;
                    }
                }
                substitutionResult = shouldAddBrowserTags && shouldEncode 
                    ? WebUtility.HtmlEncode(substitutionResult) // DO NOT CHANGE TO HttpUtility.HtmlEncode: KP-10869
                    : substitutionResult;

                substitutionResult = string.IsNullOrEmpty(substitutionResult)
                    ? this.substitutionService.DefaultSubstitutionText
                    : substitutionResult;

                textWithReplacedSubstitutions = this.substitutionService.ReplaceSubstitutionVariable(
                        textWithReplacedSubstitutions, substitution.Name, substitutionResult);
            }

            return textWithReplacedSubstitutions;
        }

        private static string MarkdownTextToHtml(string text)
        {
            using (var writer = new StringWriter())
            {
                var renderer = new HtmlRenderer(writer) { EnableHtmlForBlock = false};

                var builder = new MarkdownPipelineBuilder();

                builder.BlockParsers.Clear();
                builder.InlineParsers.Clear();

                builder.BlockParsers.AddIfNotAlready<HtmlBlockParser>();
                builder.BlockParsers.AddIfNotAlready<ParagraphBlockParser>();
                builder.InlineParsers.AddIfNotAlready<HtmlEntityParser>();
                builder.InlineParsers.AddIfNotAlready<LinkInlineParser>();
                builder.InlineParsers.AddIfNotAlready<AutolineInlineParser>();

                var pipeline = builder.Build();

                pipeline.Setup(renderer);
                var document = Markdown.Parse(text, pipeline);
                renderer.Render(document);

                return writer.ToString().TrimEnd('\n');
            }
        }

        public override string ToString() => this.Text;

        public SubstitutionText Clone() => (SubstitutionText) MemberwiseClone();
    }
}

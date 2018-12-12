using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using CommonMark;
using CommonMark.Syntax;
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
            var settings = CommonMarkSettings.Default.Clone();
            settings.OutputFormat = OutputFormat.CustomDelegate;
            settings.OutputDelegate = (doc, output, s) => new MarkdownHtmlFormatter(output, s).WriteDocument(doc, false);

            return CommonMarkConverter.Convert(text, settings);
        }

        private class MarkdownHtmlFormatter : CommonMark.Formatters.HtmlFormatter
        {
            public MarkdownHtmlFormatter(TextWriter target, CommonMarkSettings settings)
                : base(target, settings)
            {
            }

            public void WriteDocument(Block document, bool wrapToParagraph)
            {
                if (document == null)
                    throw new ArgumentNullException(nameof(document));

                if(wrapToParagraph)
                    base.WriteDocument(document);
                else
                {
                    Block ignoreUntilBlockCloses = null;
                    Inline ignoreUntilInlineCloses = null;

                    var nodes = document.AsEnumerable().Where(x => x.Block?.Tag != BlockTag.Document).ToArray();
                    foreach (var node in nodes.Skip(1 /* Autogenerated opened paragraph node */).Take(nodes.Length - 2 /* Autogenerated closed paragraph node */))
                    {
                        bool ignoreChildNodes;
                        if (node.Block != null)
                        {
                            if (ignoreUntilBlockCloses != null)
                            {
                                if (ignoreUntilBlockCloses != node.Block)
                                    continue;

                                ignoreUntilBlockCloses = null;
                            }

                            base.WriteBlock(node.Block, node.IsOpening, node.IsClosing, out ignoreChildNodes);
                            if (ignoreChildNodes && !node.IsClosing)
                                ignoreUntilBlockCloses = node.Block;
                        }
                        else if (ignoreUntilBlockCloses == null && node.Inline != null)
                        {
                            if (ignoreUntilInlineCloses != null)
                            {
                                if (ignoreUntilInlineCloses != node.Inline)
                                    continue;

                                ignoreUntilInlineCloses = null;
                            }

                            base.WriteInline(node.Inline, node.IsOpening, node.IsClosing, out ignoreChildNodes);
                            if (ignoreChildNodes && !node.IsClosing)
                                ignoreUntilInlineCloses = node.Inline;
                        }
                    }
                }
            }
        }

        public override string ToString() => this.Text;

        public SubstitutionText Clone() => (SubstitutionText) MemberwiseClone();
    }
}

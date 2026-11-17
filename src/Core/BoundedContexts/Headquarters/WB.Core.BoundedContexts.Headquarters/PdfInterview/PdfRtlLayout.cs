#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Drawing;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    /// <summary>
    /// Puts directional runs of right-to-left lines into visual order (see <see cref="PdfRtlText"/>).
    /// MigraDoc wraps lines from left to right, so only lines that surely fit into one rendered line are
    /// reordered; wrapped lines would get their parts in the wrong vertical order and are left as they are.
    /// </summary>
    internal class PdfRtlLayout
    {
        // fonts are measured approximately (sizes of table formats are not taken into account), keep a reserve
        private const double FitRatio = 0.9;
        private static readonly double DefaultCellPadding = Unit.FromMillimeter(1.2).Point;

        private readonly Document document;
        private readonly XGraphics measureContext =
            XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards);
        private readonly Dictionary<(double Size, bool Bold, bool Italic), XFont> fonts = new();

        private PdfRtlLayout(Document document)
        {
            this.document = document;
        }

        public static void ApplyVisualOrder(Document document)
        {
            var layout = new PdfRtlLayout(document);
            foreach (var section in document.Sections.OfType<Section>())
            {
                var pageSetup = section.PageSetup;
                PageSetup.GetPageSize(pageSetup.PageFormat, out var pageWidth, out _);
                var width = pageWidth.Point - pageSetup.LeftMargin.Point - pageSetup.RightMargin.Point;

                layout.Visit(section.Elements, width);
                layout.Visit(section.Headers.Primary.Elements, width);
                layout.Visit(section.Footers.Primary.Elements, width);
            }
        }

        /// <summary>
        /// Italic faces of the default fonts have no Arabic or Hebrew glyphs, such text would be rendered as boxes.
        /// Right-to-left scripts have no italics, so regular faces are used for them.
        /// </summary>
        public static void UseUprightFontForRightToLeftText(Document document)
        {
            foreach (var section in document.Sections.OfType<Section>())
            {
                UseUprightFont(section.Elements);
                UseUprightFont(section.Headers.Primary.Elements);
                UseUprightFont(section.Footers.Primary.Elements);
            }
        }

        private static void UseUprightFont(DocumentElements elements)
        {
            foreach (var element in elements)
            {
                switch (element)
                {
                    case Paragraph paragraph:
                        UseUprightFont(paragraph);
                        break;
                    case Table table:
                        foreach (var row in table.Rows.OfType<Row>())
                            for (var i = 0; i < table.Columns.Count; i++)
                                UseUprightFont(row.Cells[i].Elements);
                        break;
                    case TextFrame textFrame:
                        UseUprightFont(textFrame.Elements);
                        break;
                }
            }
        }

        private static void UseUprightFont(Paragraph paragraph)
        {
            foreach (var element in paragraph.Elements)
            {
                if (element is Text text && PdfRtlText.ContainsRightToLeft(text.Content))
                    paragraph.Format.Font.Italic = false;
                else if (element is FormattedText formattedText && formattedText.Elements.OfType<Text>()
                             .Any(child => PdfRtlText.ContainsRightToLeft(child.Content)))
                    formattedText.Font.Italic = false;
            }
        }

        private void Visit(DocumentElements elements, double width)
        {
            foreach (var element in elements)
            {
                switch (element)
                {
                    case Paragraph paragraph:
                        Reorder(paragraph, width - GetIndents(paragraph));
                        break;
                    case Table table:
                        foreach (var row in table.Rows.OfType<Row>())
                        {
                            for (var i = 0; i < table.Columns.Count; i++)
                            {
                                var column = table.Columns[i];
                                var padding = Padding(column.LeftPadding, table.LeftPadding)
                                              + Padding(column.RightPadding, table.RightPadding);
                                Visit(row.Cells[i].Elements, column.Width.Point - padding);
                            }
                        }
                        break;
                    case TextFrame textFrame:
                        Visit(textFrame.Elements, textFrame.Width.Point);
                        break;
                }
            }
        }

        private void Reorder(Paragraph paragraph, double width)
        {
            var elements = paragraph.Elements.OfType<DocumentObject>().ToList();
            var result = new List<DocumentObject>();
            var changed = false;

            var segmentStart = 0;
            for (var i = 0; i <= elements.Count; i++)
            {
                if (i < elements.Count && !IsLineBreak(elements[i]))
                    continue;

                var segment = elements.GetRange(segmentStart, i - segmentStart);
                var reordered = ReorderLine(paragraph, segment, width);
                changed |= reordered != null;
                result.AddRange(reordered ?? segment);
                if (i < elements.Count)
                    result.Add(elements[i]);
                segmentStart = i + 1;
            }

            if (!changed)
                return;

            paragraph.Elements.Clear();
            // MigraDoc does not allow to insert an element twice, even after the collection was cleared
            foreach (var element in result)
                paragraph.Elements.Add((DocumentObject)element.Clone());
        }

        private List<DocumentObject>? ReorderLine(Paragraph paragraph, List<DocumentObject> line, double width)
        {
            var leading = new List<DocumentObject>();
            var pieces = new List<(string Text, FormattedText? Owner)>();
            foreach (var element in line)
            {
                switch (element)
                {
                    case BookmarkField:
                        leading.Add(element);
                        break;
                    case FormattedText formattedText:
                        // chunks of one text are added as separate formatted texts with the same formatting
                        var owner = pieces.Select(piece => piece.Owner)
                            .FirstOrDefault(other => other != null && HasSameFormat(other, formattedText)) ?? formattedText;
                        foreach (var child in formattedText.Elements.OfType<DocumentObject>())
                        {
                            var text = GetText(child);
                            if (text == null)
                                return null;
                            pieces.Add((text, owner));
                        }
                        break;
                    default:
                        var plainText = GetText(element);
                        if (plainText == null)
                            return null;
                        pieces.Add((plainText, null));
                        break;
                }
            }

            if (!PdfRtlText.StartsRightToLeft(string.Concat(pieces.Select(piece => piece.Text))))
                return null;

            var lineWidth = pieces.Sum(piece => measureContext.MeasureString(piece.Text, GetFont(paragraph, piece.Owner)).Width);
            if (lineWidth > width * FitRatio)
                return null;

            var result = new List<DocumentObject>(leading);
            foreach (var (text, owner) in PdfRtlText.ToVisualOrder(pieces))
            {
                if (owner == null)
                {
                    result.Add(new Text(text));
                    continue;
                }

                var formattedText = owner.Clone();
                formattedText.Elements.Clear();
                formattedText.AddText(text);
                result.Add(formattedText);
            }

            return result;
        }

        private static bool HasSameFormat(FormattedText left, FormattedText right) =>
            left.Style == right.Style
            && left.Font.Name == right.Font.Name
            && left.Font.Size == right.Font.Size
            && left.Font.Bold == right.Font.Bold
            && left.Font.Italic == right.Font.Italic
            && left.Font.Underline == right.Font.Underline
            && left.Font.Color.Argb == right.Font.Color.Argb
            && left.Color.Argb == right.Color.Argb;

        private static string? GetText(DocumentObject element) => element switch
        {
            Text text => text.Content,
            Character { SymbolName: SymbolName.Blank } character =>
                new string(' ', Math.Max(1, character.Count)),
            _ => null
        };

        private static bool IsLineBreak(DocumentObject element) =>
            element is Character { SymbolName: SymbolName.LineBreak };

        private static double Padding(Unit columnPadding, Unit tablePadding) =>
            !columnPadding.IsEmpty ? columnPadding.Point : !tablePadding.IsEmpty ? tablePadding.Point : DefaultCellPadding;

        private double GetIndents(Paragraph paragraph)
        {
            var format = paragraph.Values.Format?.Values;
            var left = format?.LeftIndent ?? FindInStyles(paragraph.Style ?? string.Empty, style => style.ParagraphFormat.Values.LeftIndent);
            var right = format?.RightIndent ?? FindInStyles(paragraph.Style ?? string.Empty, style => style.ParagraphFormat.Values.RightIndent);
            return (left?.Point ?? 0) + (right?.Point ?? 0);
        }

        private XFont GetFont(Paragraph paragraph, FormattedText? owner)
        {
            var ownerFont = owner?.Values.Font?.Values;
            var paragraphFont = paragraph.Values.Format?.Values.Font?.Values;

            // explicit font of the formatted text, its character style, explicit font of the paragraph, paragraph style
            var ownerStyle = string.IsNullOrEmpty(owner?.Style) ? null : owner.Style;
            var size = ownerFont?.Size ?? FindInStyles(ownerStyle, style => style.Font.Values.Size)
                       ?? paragraphFont?.Size ?? FindInStyles(paragraph.Style ?? string.Empty, style => style.Font.Values.Size);
            var bold = ownerFont?.Bold ?? FindInStyles(ownerStyle, style => style.Font.Values.Bold)
                       ?? paragraphFont?.Bold ?? FindInStyles(paragraph.Style ?? string.Empty, style => style.Font.Values.Bold) ?? false;
            var italic = ownerFont?.Italic ?? FindInStyles(ownerStyle, style => style.Font.Values.Italic)
                         ?? paragraphFont?.Italic ?? FindInStyles(paragraph.Style ?? string.Empty, style => style.Font.Values.Italic) ?? false;

            var key = (size?.Point ?? 10, bold, italic);
            if (!fonts.TryGetValue(key, out var font))
            {
                var style = (bold ? XFontStyleEx.Bold : XFontStyleEx.Regular) | (italic ? XFontStyleEx.Italic : XFontStyleEx.Regular);
                font = new XFont(DefinePdfStyles.DefaultFonts, key.Item1, style);
                fonts[key] = font;
            }

            return font;
        }

        /// <summary>Finds a value in the style or in its base styles; an empty paragraph style means Normal.</summary>
        private TValue? FindInStyles<TValue>(string? styleName, Func<Style, TValue?> getValue) where TValue : struct
        {
            if (styleName == null)
                return null;

            var style = document.Styles[styleName.Length == 0 ? StyleNames.Normal : styleName];
            while (style != null)
            {
                var value = getValue(style);
                if (value != null && !(value is Unit unit && unit.IsEmpty))
                    return value;

                style = string.IsNullOrEmpty(style.BaseStyle) ? null : document.Styles[style.BaseStyle];
            }

            return null;
        }
    }
}

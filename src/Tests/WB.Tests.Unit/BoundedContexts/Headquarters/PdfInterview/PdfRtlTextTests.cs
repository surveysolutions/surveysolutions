using System.Linq;
using System.Runtime.CompilerServices;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(PdfRtlText))]
public class PdfRtlTextTests
{
    [TestCase("أسئلة غير مجابة", "أسئلة غير مجابة")]
    [TestCase("33 أقسام", "أقسام 33")]
    [TestCase("557 أسئلة غير مجابة", "أسئلة غير مجابة 557")]
    [TestCase("مفتاح المقابلة: 03-48-65-67", "03-48-65-67 :مفتاح المقابلة")]
    [TestCase("تم إنشاؤه بواسطة http://localhost:5001/primary", "http://localhost:5001/primary تم إنشاؤه بواسطة")]
    [TestCase("في 2026-11-17 20:21 +00:00", "2026-11-17 20:21 +00:00 في")]
    [TestCase("(تعيين #939)", "(939# تعيين)")]
    public void when_reordering_right_to_left_line_should_reverse_directional_runs(string logical, string visual)
    {
        Assert.That(PdfRtlText.ToVisualOrder(logical), Is.EqualTo(visual));
    }

    [TestCase("مفتاح 1", true)]
    [TestCase("12 مفتاح", true)]
    [TestCase("Interview مفتاح", false)]
    [TestCase("2026-11-17", false)]
    public void when_detecting_line_direction_should_use_first_letter(string text, bool isRightToLeft)
    {
        Assert.That(PdfRtlText.StartsRightToLeft(text), Is.EqualTo(isRightToLeft));
    }

    [Test]
    public void when_right_to_left_run_has_parts_with_different_formatting_should_reverse_parts()
    {
        var pieces = new[] { ("حجم ", "title"), ("المقابلة", "bold") };

        var visual = PdfRtlText.ToVisualOrder(pieces);

        Assert.That(visual, Is.EqualTo(new[] { ("المقابلة", "bold"), (" حجم", "title") }));
    }

    [Test]
    public void when_reordering_chunks_with_same_formatting_should_merge_them_into_one_run()
    {
        var pieces = new[] { ("أسئلة مجا", "a"), ("بة", "a"), (" 12", "a") };

        var visual = PdfRtlText.ToVisualOrder(pieces);

        Assert.That(visual, Is.EqualTo(new[] { ("12 أسئلة مجابة", "a") }));
    }
}

[TestOf(typeof(ParagraphExtensions))]
public class ParagraphExtensionsRtlTests
{
    [Test]
    public void when_adding_wrapped_text_should_split_only_left_to_right_text()
    {
        var paragraph = new Document().AddSection().AddParagraph();

        paragraph.AddWrapFormattedText(new string('a', 30), PdfStyles.Default);
        paragraph.AddWrapFormattedText("صف ظروف السكن في المنزل بالتفصيل", PdfStyles.Default);

        var texts = paragraph.Elements.OfType<FormattedText>()
            .Select(text => string.Concat(text.Elements.OfType<Text>().Select(t => t.Content))).ToArray();
        Assert.That(texts, Is.EqualTo(new[] { new string('a', 15), new string('a', 15), "صف ظروف السكن في المنزل بالتفصيل" }));
    }
}

[TestOf(typeof(PdfRtlLayout))]
public class PdfRtlLayoutTests
{
    [OneTimeSetUp]
    public void SetUpFonts() => RuntimeHelpers.RunClassConstructor(typeof(PdfInterviewGenerator).TypeHandle);

    [Test]
    public void when_applying_visual_order_should_reorder_short_right_to_left_lines_only()
    {
        var document = CreateDocument(out var section);
        var label = section.AddParagraph();
        label.AddFormattedText("557 أسئلة غير مجابة", TextFormat.Bold);
        label.AddLineBreak();
        label.AddText("English line 1");

        var longLine = section.AddParagraph();
        longLine.AddText(string.Join(" ", Enumerable.Repeat("مقابلة", 200)) + " 2026");

        var withField = section.AddParagraph();
        withField.AddText("صفحة 1 ");
        withField.AddPageField();

        PdfRtlLayout.ApplyVisualOrder(document);

        Assert.That(GetLines(label), Is.EqualTo(new[] { "أسئلة غير مجابة 557", "English line 1" }));
        Assert.That(label.Elements.OfType<FormattedText>().All(text => text.Bold), Is.True);
        Assert.That(GetLines(longLine).Single(), Does.EndWith(" 2026"));
        Assert.That(withField.Elements.OfType<PageField>().Count(), Is.EqualTo(1));
        Assert.That(GetLines(withField).Single(), Is.EqualTo("صفحة 1 "));
    }

    [Test]
    public void when_applying_visual_order_should_keep_bookmark_of_section_title()
    {
        var document = CreateDocument(out var section);
        var title = section.AddParagraph();
        title.AddBookmark("القسم 1");
        title.AddText("القسم 1");

        PdfRtlLayout.ApplyVisualOrder(document);

        Assert.That(title.Elements[0], Is.InstanceOf<BookmarkField>());
        Assert.That(GetLines(title).Single(), Is.EqualTo("1 القسم"));
    }

    [Test]
    public void when_using_upright_font_should_reset_italic_only_for_right_to_left_text()
    {
        var document = CreateDocument(out var section);
        var paragraph = section.AddParagraph();
        var arabic = paragraph.AddFormattedText("لم تتم الإجابة", PdfStyles.QuestionNotAnswered);
        var english = paragraph.AddFormattedText("Not answered", PdfStyles.QuestionNotAnswered);
        var styled = section.AddParagraph();
        styled.Style = PdfStyles.CommentAuthor;
        styled.AddText("المشرف");

        PdfRtlLayout.UseUprightFontForRightToLeftText(document);

        Assert.That(arabic.Font.Italic, Is.False);
        Assert.That(english.Values.Font?.Values.Italic, Is.Null);
        Assert.That(styled.Format.Font.Italic, Is.False);
    }

    private static Document CreateDocument(out Section section)
    {
        var document = new Document();
        new DefinePdfStyles().Define(document, isRightToLeft: true);
        section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.LeftMargin = 33;
        section.PageSetup.RightMargin = 37;
        return document;
    }

    private static string[] GetLines(Paragraph paragraph)
    {
        var lines = new[] { "" }.ToList();
        foreach (var element in paragraph.Elements)
        {
            switch (element)
            {
                case Character { SymbolName: SymbolName.LineBreak }:
                    lines.Add("");
                    break;
                case Text text:
                    lines[^1] += text.Content;
                    break;
                case FormattedText formattedText:
                    lines[^1] += string.Concat(formattedText.Elements.OfType<Text>().Select(text => text.Content));
                    break;
            }
        }

        return lines.ToArray();
    }
}

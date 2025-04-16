using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview;

public record FontScriptPart(string fragment, string font)
{
    public string Fragment => fragment;
    public string Font => $"{font}, {DefinePdfStyles.DefaultFonts}";
}

public static class FontScriptDetector
{
    private static readonly List<(string Font, int Min, int Max)> ScriptRanges = new()
    {
        ("Noto Sans", 0x0000, 0x007F),                      // Basic Latin
        ("Noto Sans", 0x0400, 0x04FF),                      // Cyrillic

        ("Noto Sans Greek", 0x0370, 0x03FF),
        ("Noto Sans Hebrew", 0x0590, 0x05FF),
        ("Noto Sans Arabic", 0x0600, 0x06FF),
        ("Noto Sans Arabic Extended-A", 0x0750, 0x077F),
        ("Noto Sans Arabic Extended-B", 0x08A0, 0x08FF),
        ("Noto Sans Arabic Presentation Forms-A", 0xFB1D, 0xFB4F),
        ("Noto Sans Arabic Presentation Forms-B", 0xFE70, 0xFEFF),
        ("Noto Sans Thaana", 0x0780, 0x07BF),     

        ("Noto Sans Devanagari", 0x0900, 0x097F),
        ("Noto Sans Bengali", 0x0980, 0x09FF),
        ("Noto Sans Gujarati", 0x0A80, 0x0AFF),
        ("Noto Sans Oriya", 0x0B00, 0x0B7F),
        ("Noto Sans Tamil", 0x0B80, 0x0BFF),
        ("Noto Sans Telugu", 0x0C00, 0x0C7F),
        ("Noto Sans Kannada", 0x0C80, 0x0CFF),
        ("Noto Sans Malayalam", 0x0D00, 0x0D7F),
        ("Noto Sans Sinhala", 0x0D80, 0x0DFF),
        ("Noto Sans Thai", 0x0E00, 0x0E7F),
        ("Noto Sans Lao", 0x0E80, 0x0EFF),
        ("Noto Sans Tibetan", 0x0F00, 0x0FFF),

        ("Noto Sans Myanmar", 0x1000, 0x109F),
        ("Noto Sans Myanmar Extended-A", 0xAA00, 0xAA5F),
        ("Noto Sans Myanmar Extended-B", 0xAA60, 0xAA7F),

        ("Noto Sans Georgian", 0x10A0, 0x10FF),
        ("Noto Sans Ethiopic", 0x1200, 0x137F),
        ("Noto Sans Ethiopic Extended", 0x1380, 0x139F),
        ("Noto Sans Ethiopic Supplement", 0x2D80, 0x2DDF),

        ("Noto Sans Cherokee", 0x13A0, 0x13FF),
        ("Noto Sans Canadian Aboriginal", 0x1400, 0x167F),
        ("Noto Sans Ogham", 0x1680, 0x169F),
        ("Noto Sans Runic", 0x16A0, 0x16FF),
        ("Noto Sans Tagalog", 0x1700, 0x171F),
        ("Noto Sans Hanunoo", 0x1720, 0x173F),
        ("Noto Sans Buhid", 0x1740, 0x175F),
        ("Noto Sans Tagbanua", 0x1760, 0x177F),
        ("Noto Sans Khmer", 0x1780, 0x17FF),

        ("Noto Sans Limbu", 0x1900, 0x194F),
        ("Noto Sans Tai Le", 0x1950, 0x197F),

        ("Noto Sans Buginese", 0x1A00, 0x1A1F),
        ("Noto Sans Balinese", 0x1B00, 0x1B7F),
        ("Noto Sans Sundanese", 0x1B80, 0x1BBF),
        ("Noto Sans Batak", 0x1BC0, 0x1BFF),

        ("Noto Sans Glagolitic", 0x2C00, 0x2C5F),
        ("Noto Sans Coptic", 0x2C80, 0x2CFF),

        ("Noto Sans Bopomofo", 0x3100, 0x312F),

        ("Noto Sans JP", 0x3040, 0x30FF),                  // Hiragana/Katakana
        ("Noto Sans CJK SC", 0x4E00, 0x9FFF),              // Unified Han
        ("Noto Sans KR", 0xAC00, 0xD7AF),                  // Hangul Syllables

        ("Noto Sans Yi", 0xA000, 0xA4CF),
        ("Noto Sans Javanese", 0xA980, 0xA9DF),

        ("Noto Sans Meetei Mayek", 0xAA80, 0xAADF),

        ("Noto Sans Gothic", 0x10330, 0x1034F),
        ("Noto Sans Ugaritic", 0x10380, 0x1039F),
        ("Noto Sans Shavian", 0x10450, 0x1047F),
        ("Noto Sans Deseret", 0x10400, 0x1044F),

        // Fallback for any undefined ranges
        ("Noto Sans", 0x10000, 0x10FFFF)
    };

    private static string GetFontForChar(char ch)
    {
        int code = ch;
        foreach (var (font, min, max) in ScriptRanges)
        {
            if (code >= min && code <= max)
                return font;
        }
        return "Noto Sans"; // fallback
    }

    public static List<FontScriptPart> SplitByScript(string input)
    {
        var result = new List<FontScriptPart>();
        if (string.IsNullOrEmpty(input)) return result;

        string currentFont = GetFontForChar(input[0]);
        string buffer = "";

        foreach (char ch in input)
        {
            string font = GetFontForChar(ch);

            if (font != currentFont)
            {
                result.Add(new FontScriptPart(buffer, currentFont));
                buffer = ch.ToString();
                currentFont = font;
            }
            else
            {
                buffer += ch;
            }
        }

        if (!string.IsNullOrEmpty(buffer))
            result.Add(new FontScriptPart(buffer, currentFont));

        return result;
    }
}

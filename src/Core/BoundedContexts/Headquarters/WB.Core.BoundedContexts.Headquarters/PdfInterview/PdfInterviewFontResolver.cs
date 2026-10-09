#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Fonts;
using SkiaSharp;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    /// <summary>
    /// Resolves TrueType fonts installed in the system (or shipped with the product) for PDFsharp.
    /// Font family could be a comma separated list, the first installed family is used.
    /// </summary>
    public class PdfInterviewFontResolver : IFontResolver
    {
        private static readonly string[] DefaultFamilies = { "Arial", "Noto Sans", "DejaVu Sans" };

        private readonly Lazy<Dictionary<(string family, bool bold, bool italic), string>> fonts;

        public PdfInterviewFontResolver() : this(GetSystemFontDirectories())
        {
        }

        public PdfInterviewFontResolver(IEnumerable<string> fontDirectories)
        {
            fonts = new Lazy<Dictionary<(string, bool, bool), string>>(() => ScanFonts(fontDirectories.ToList()));
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            foreach (var fontName in familyName.Split(','))
            {
                var info = ResolveFamily(fontName.Trim(), isBold, isItalic);
                if (info != null)
                    return info;
            }

            foreach (var defaultFamily in DefaultFamilies)
            {
                var info = ResolveFamily(defaultFamily, isBold, isItalic);
                if (info != null)
                    return info;
            }

            var any = fonts.Value.FirstOrDefault();
            return any.Value == null ? null : new FontResolverInfo(any.Value);
        }

        public byte[]? GetFont(string faceName) => File.ReadAllBytes(faceName);

        private FontResolverInfo? ResolveFamily(string family, bool isBold, bool isItalic)
        {
            if (string.IsNullOrWhiteSpace(family))
                return null;

            var key = family.ToLowerInvariant();

            if (fonts.Value.TryGetValue((key, isBold, isItalic), out var exact))
                return new FontResolverInfo(exact);

            // requested style is not installed, closest available face is used with simulated style
            foreach (var (bold, italic) in new[] { (isBold, false), (false, isItalic), (false, false) })
            {
                if (fonts.Value.TryGetValue((key, bold, italic), out var fallback))
                    return new FontResolverInfo(fallback, isBold && !bold, isItalic && !italic);
            }

            return null;
        }

        private static Dictionary<(string family, bool bold, bool italic), string> ScanFonts(List<string> fontDirectories)
        {
            var result = new Dictionary<(string, bool, bool), string>();

            foreach (var directory in fontDirectories.Where(Directory.Exists))
            {
                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(directory, "*.ttf", new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true
                    }).ToList();
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (var file in files)
                {
                    using var typeface = SKTypeface.FromFile(file);
                    if (typeface == null)
                        continue;

                    var key = (typeface.FamilyName.ToLowerInvariant(),
                        typeface.FontWeight >= (int)SKFontStyleWeight.SemiBold,
                        typeface.FontSlant != SKFontStyleSlant.Upright);

                    result.TryAdd(key, file);
                }
            }

            return result;
        }

        private static IEnumerable<string> GetSystemFontDirectories()
        {
            if (OperatingSystem.IsWindows())
            {
                yield return Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
                yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "Windows", "Fonts");
                yield break;
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (OperatingSystem.IsMacOS())
            {
                yield return "/Library/Fonts";
                yield return "/System/Library/Fonts";
                yield return Path.Combine(home, "Library", "Fonts");
                yield break;
            }

            yield return "/usr/share/fonts";
            yield return "/usr/local/share/fonts";
            yield return Path.Combine(home, ".fonts");
            yield return Path.Combine(home, ".local", "share", "fonts");
        }
    }
}

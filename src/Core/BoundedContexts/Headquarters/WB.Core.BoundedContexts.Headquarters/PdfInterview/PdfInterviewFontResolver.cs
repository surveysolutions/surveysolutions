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
    /// Among faces of a family the one with the closest weight is used (400 for regular, 700 for bold),
    /// faces with a non-normal width (condensed, expanded) are ignored while there are faces with a normal width.
    /// </summary>
    public class PdfInterviewFontResolver : IFontResolver
    {
        private static readonly string[] DefaultFamilies = { "Arial", "Noto Sans", "DejaVu Sans" };

        // PDFsharp can not use CFF based OpenType fonts, true type collections are split to separate faces
        private static readonly string[] FontExtensions = { ".ttf", ".ttc" };

        private const int RegularWeight = (int)SKFontStyleWeight.Normal;
        private const int BoldWeight = (int)SKFontStyleWeight.Bold;
        private const int MaxFacesInCollection = 256;

        private sealed record FontFace(
            string FaceName,
            string Path,
            int CollectionIndex,
            bool IsCollection,
            string Family,
            int Weight,
            int Width,
            bool Italic);

        private readonly Lazy<List<FontFace>> faces;
        private readonly Lazy<Dictionary<string, FontFace>> facesByName;

        public PdfInterviewFontResolver() : this(GetSystemFontDirectories())
        {
        }

        public PdfInterviewFontResolver(IEnumerable<string> fontDirectories)
        {
            faces = new Lazy<List<FontFace>>(() => ScanFonts(fontDirectories.ToList()));
            facesByName = new Lazy<Dictionary<string, FontFace>>(() => faces.Value.ToDictionary(face => face.FaceName));
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

            var any = faces.Value.FirstOrDefault();
            return any == null
                ? null
                : ResolveFamily(any.Family, isBold, isItalic) ?? new FontResolverInfo(any.FaceName);
        }

        public byte[]? GetFont(string faceName)
        {
            // only faces found by the resolver are served, any other path must not be read
            if (!facesByName.Value.TryGetValue(faceName, out var face))
                return null;

            return face.IsCollection
                ? FontFileExtractor.ExtractFace(face.Path, face.CollectionIndex)
                : File.ReadAllBytes(face.Path);
        }

        private FontResolverInfo? ResolveFamily(string family, bool isBold, bool isItalic)
        {
            if (string.IsNullOrWhiteSpace(family))
                return null;

            var candidates = faces.Value
                .Where(face => string.Equals(face.Family, family, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (candidates.Count == 0)
                return null;

            // condensed or expanded faces have the same family name, but must not replace the normal ones
            var normalWidth = candidates.Where(face => face.Width == (int)SKFontStyleWidth.Normal).ToList();
            if (normalWidth.Count > 0)
                candidates = normalWidth;

            // sorting is stable: faces with equal style stay in the order of the font directories and file names
            var best = candidates
                .OrderBy(face => WeightRank(face.Weight, isBold))
                .ThenBy(face => face.Italic == isItalic ? 0 : 1)
                .First();

            // requested style is not installed, closest available face is used with simulated style
            return new FontResolverInfo(best.FaceName,
                isBold && best.Weight < (int)SKFontStyleWeight.SemiBold,
                isItalic && !best.Italic);
        }

        /// <summary>
        /// Orders weights by closeness to the requested one, in the way css does it:
        /// for regular 400, then 500, then lighter ones, then heavier ones; for bold 700 or heavier, then lighter ones.
        /// </summary>
        private static int WeightRank(int weight, bool isBold)
        {
            if (isBold)
                return weight >= BoldWeight ? weight - BoldWeight : 10_000 + (BoldWeight - weight);

            if (weight is >= RegularWeight and <= (int)SKFontStyleWeight.Medium)
                return weight - RegularWeight;

            return weight < RegularWeight ? 10_000 + (RegularWeight - weight) : 20_000 + weight;
        }

        private static List<FontFace> ScanFonts(List<string> fontDirectories)
        {
            var result = new List<FontFace>();
            var foundFaces = new HashSet<string>();

            foreach (var directory in fontDirectories.Where(Directory.Exists))
            {
                List<string> files;
                try
                {
                    // extension is compared here and not by a search pattern, because the pattern is case sensitive on Linux
                    files = Directory.EnumerateFiles(directory, "*", new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true
                    })
                    .Where(file => FontExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    .OrderBy(file => file, StringComparer.Ordinal)
                    .ToList();
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                {
                    continue;
                }

                foreach (var file in files)
                {
                    foreach (var face in ReadFaces(file))
                    {
                        if (foundFaces.Add(face.FaceName))
                            result.Add(face);
                    }
                }
            }

            return result;
        }

        private static IEnumerable<FontFace> ReadFaces(string file)
        {
            var isCollection = string.Equals(Path.GetExtension(file), ".ttc", StringComparison.OrdinalIgnoreCase);
            var count = isCollection ? FontFileExtractor.GetCollectionSize(file) : 1;

            for (var index = 0; index < Math.Min(count, MaxFacesInCollection); index++)
            {
                using var typeface = SKTypeface.FromFile(file, index);

                // PDFsharp can use only fonts with true type outlines
                if (typeface == null || !FontFileExtractor.HasTrueTypeOutlines(typeface))
                    continue;

                yield return new FontFace(
                    isCollection ? $"{file}#{index}" : file,
                    file,
                    index,
                    isCollection,
                    typeface.FamilyName,
                    typeface.FontWeight,
                    typeface.FontWidth,
                    typeface.FontSlant != SKFontStyleSlant.Upright);
            }
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

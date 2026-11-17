#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    /// <summary>
    /// Bidi reordering of a right-to-left line for MigraDoc.
    /// PDFsharp orders text of every formatted run as a left-to-right paragraph: right-to-left words
    /// of a run are joined and reversed in place, while numbers and Latin words around them keep the logical order.
    /// So for a right-to-left line the order of directional runs is reversed here, and parts of a right-to-left run
    /// that have different formatting are reversed, because PDFsharp reverses each of them separately.
    /// </summary>
    internal static class PdfRtlText
    {
        private enum CharClass { RightToLeft, LeftToRight, Whitespace, Neutral }

        private sealed record Token(int Start, int End, CharClass Class);

        private static readonly Dictionary<char, char> MirroredChars = new()
        {
            ['('] = ')', [')'] = '(', ['['] = ']', [']'] = '[', ['{'] = '}', ['}'] = '{',
            ['<'] = '>', ['>'] = '<', ['«'] = '»', ['»'] = '«',
        };

        public static bool IsRightToLeftChar(char c) =>
            (c is >= '\u0590' and <= '\u08FF' && !IsArabicDigit(c))
            || c is >= '\uFB1D' and <= '\uFDFF'
            || c is >= '\uFE70' and <= '\uFEFF';

        public static bool ContainsRightToLeft(string text) => text.Any(IsRightToLeftChar);

        /// <summary>Direction of a line is the direction of its first letter.</summary>
        public static bool StartsRightToLeft(string text)
        {
            foreach (var c in text)
            {
                if (IsRightToLeftChar(c))
                    return true;
                if (char.IsLetter(c))
                    return false;
            }

            return false;
        }

        public static string ToVisualOrder(string text) =>
            string.Concat(ToVisualOrder(new[] { (text, 0) }).Select(piece => piece.Text));

        /// <summary>
        /// Reorders pieces of a right-to-left line. A piece keeps its owner (formatting); pieces with equal owners
        /// are merged, so PDFsharp gets every right-to-left run of the same formatting as a single text.
        /// </summary>
        public static List<(string Text, T Owner)> ToVisualOrder<T>(IReadOnlyList<(string Text, T Owner)> pieces)
        {
            var text = new StringBuilder();
            var owners = new List<T>();
            foreach (var piece in pieces)
            {
                text.Append(piece.Text);
                owners.AddRange(Enumerable.Repeat(piece.Owner, piece.Text.Length));
            }

            var tokens = Tokenize(text.ToString());
            var items = new List<(Direction Direction, List<Token> Tokens)>();
            for (var i = 0; i < tokens.Count; i++)
            {
                var direction = Resolve(tokens, i);
                if (direction != Direction.Neutral && items.Count > 0 && items[^1].Direction == direction)
                    items[^1].Tokens.Add(tokens[i]);
                else
                    items.Add((direction, new List<Token> { tokens[i] }));
            }

            var visual = new List<(char Char, T Owner)>();
            for (var i = items.Count - 1; i >= 0; i--)
            {
                var (direction, itemTokens) = items[i];
                var start = itemTokens[0].Start;
                var end = itemTokens[^1].End;
                switch (direction)
                {
                    case Direction.RightToLeft:
                        AddRightToLeftRun(visual, text, owners, start, end);
                        break;
                    case Direction.Neutral when itemTokens[0].Class == CharClass.Neutral:
                        // neutrals between runs of different directions follow the right-to-left line
                        for (var c = end - 1; c >= start; c--)
                            visual.Add((MirroredChars.TryGetValue(text[c], out var mirrored) ? mirrored : text[c], owners[c]));
                        break;
                    default:
                        for (var c = start; c < end; c++)
                            visual.Add((text[c], owners[c]));
                        break;
                }
            }

            var result = new List<(string Text, T Owner)>();
            var current = new StringBuilder();
            for (var i = 0; i < visual.Count; i++)
            {
                current.Append(visual[i].Char);
                if (i == visual.Count - 1 || !Equals(visual[i + 1].Owner, visual[i].Owner))
                {
                    result.Add((current.ToString(), visual[i].Owner));
                    current.Clear();
                }
            }

            return result;
        }

        private static void AddRightToLeftRun<T>(List<(char Char, T Owner)> visual, StringBuilder text, List<T> owners,
            int start, int end)
        {
            var parts = new List<(int Start, int End)>();
            for (var c = start; c < end; c++)
            {
                if (c == start || !Equals(owners[c], owners[c - 1]))
                    parts.Add((c, c + 1));
                else
                    parts[^1] = (parts[^1].Start, c + 1);
            }

            for (var p = parts.Count - 1; p >= 0; p--)
            {
                var (partStart, partEnd) = parts[p];
                var trimmedStart = partStart;
                while (trimmedStart < partEnd && char.IsWhiteSpace(text[trimmedStart]))
                    trimmedStart++;
                var trimmedEnd = partEnd;
                while (trimmedEnd > trimmedStart && char.IsWhiteSpace(text[trimmedEnd - 1]))
                    trimmedEnd--;

                // spaces at the edges of a part separate it from the neighbour parts, which change their places
                for (var c = trimmedEnd; c < partEnd; c++)
                    visual.Add((text[c], owners[c]));
                for (var c = trimmedStart; c < trimmedEnd; c++)
                    visual.Add((text[c], owners[c]));
                for (var c = partStart; c < trimmedStart; c++)
                    visual.Add((text[c], owners[c]));
            }
        }

        private static bool Equals<T>(T left, T right) => EqualityComparer<T>.Default.Equals(left, right);

        private enum Direction { LeftToRight, RightToLeft, Neutral }

        private static Direction Resolve(List<Token> tokens, int index)
        {
            switch (tokens[index].Class)
            {
                case CharClass.LeftToRight:
                    return Direction.LeftToRight;
                case CharClass.RightToLeft:
                    return Direction.RightToLeft;
            }

            var previous = NearestStrongClass(tokens, index, -1);
            var next = NearestStrongClass(tokens, index, 1);
            if (previous == next && previous == CharClass.LeftToRight)
                return Direction.LeftToRight;
            if (previous == next && previous == CharClass.RightToLeft)
                return Direction.RightToLeft;
            return Direction.Neutral;
        }

        private static CharClass? NearestStrongClass(List<Token> tokens, int index, int step)
        {
            for (var i = index + step; i >= 0 && i < tokens.Count; i += step)
            {
                if (tokens[i].Class is CharClass.RightToLeft or CharClass.LeftToRight)
                    return tokens[i].Class;
            }

            return null;
        }

        private static List<Token> Tokenize(string text)
        {
            var tokens = new List<Token>();
            var start = 0;
            for (var i = 1; i <= text.Length; i++)
            {
                if (i < text.Length && Classify(text[i]) == Classify(text[start]))
                    continue;

                tokens.Add(new Token(start, i, Classify(text[start])));
                start = i;
            }

            return tokens;
        }

        private static CharClass Classify(char c)
        {
            if (IsRightToLeftChar(c))
                return CharClass.RightToLeft;
            if (char.IsWhiteSpace(c))
                return CharClass.Whitespace;
            return char.IsLetterOrDigit(c) || IsArabicDigit(c) || char.IsSurrogate(c)
                ? CharClass.LeftToRight
                : CharClass.Neutral;
        }

        private static bool IsArabicDigit(char c) => c is >= '\u0660' and <= '\u0669' or >= '\u06F0' and <= '\u06F9';
    }
}

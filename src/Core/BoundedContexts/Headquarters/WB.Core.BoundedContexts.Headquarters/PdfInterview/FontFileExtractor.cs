#nullable enable

using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    /// <summary>
    /// PDFsharp can not select a face in a true type collection (.ttc), so a face is returned as a standalone font file.
    /// </summary>
    internal static class FontFileExtractor
    {
        private static readonly uint CollectionTag = Tag("ttcf");
        private static readonly uint GlyphTableTag = Tag("glyf");
        private const uint TrueTypeVersion = 0x00010000;

        /// <summary>Number of faces in the collection, 0 when the file is not a collection.</summary>
        public static int GetCollectionSize(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                Span<byte> header = stackalloc byte[12];
                if (stream.Read(header) != header.Length || BinaryPrimitives.ReadUInt32BigEndian(header) != CollectionTag)
                    return 0;

                var count = BinaryPrimitives.ReadUInt32BigEndian(header[8..]);
                return count > int.MaxValue ? int.MaxValue : (int)count;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                return 0;
            }
        }

        public static bool HasTrueTypeOutlines(SKTypeface typeface) => typeface.GetTableTags().Contains(GlyphTableTag);

        /// <summary>Builds a standalone true type font file from tables of the face.</summary>
        public static byte[]? ExtractFace(string path, int collectionIndex)
        {
            using var typeface = SKTypeface.FromFile(path, collectionIndex);
            if (typeface == null)
                return null;

            var tables = typeface.GetTableTags()
                .OrderBy(tag => tag)
                .Select(tag => (tag, data: typeface.TryGetTableData(tag, out var data) ? data : null))
                .Where(table => table.data != null)
                .Select(table => (table.tag, data: table.data!))
                .ToList();
            if (tables.Count == 0)
                return null;

            var entrySelector = (int)Math.Log2(tables.Count);
            var searchRange = 16 * (1 << entrySelector);
            var tableRecordsEnd = 12 + 16 * tables.Count;

            var font = new byte[tableRecordsEnd + tables.Sum(table => Align4(table.data.Length))];
            BinaryPrimitives.WriteUInt32BigEndian(font, TrueTypeVersion);
            BinaryPrimitives.WriteUInt16BigEndian(font.AsSpan(4), (ushort)tables.Count);
            BinaryPrimitives.WriteUInt16BigEndian(font.AsSpan(6), (ushort)searchRange);
            BinaryPrimitives.WriteUInt16BigEndian(font.AsSpan(8), (ushort)entrySelector);
            BinaryPrimitives.WriteUInt16BigEndian(font.AsSpan(10), (ushort)(16 * tables.Count - searchRange));

            var dataOffset = tableRecordsEnd;
            for (var i = 0; i < tables.Count; i++)
            {
                var (tag, data) = tables[i];
                var record = font.AsSpan(12 + 16 * i);
                BinaryPrimitives.WriteUInt32BigEndian(record, tag);
                BinaryPrimitives.WriteUInt32BigEndian(record[4..], Checksum(data));
                BinaryPrimitives.WriteUInt32BigEndian(record[8..], (uint)dataOffset);
                BinaryPrimitives.WriteUInt32BigEndian(record[12..], (uint)data.Length);

                data.CopyTo(font, dataOffset);
                dataOffset += Align4(data.Length);
            }

            return font;
        }

        private static int Align4(int length) => (length + 3) & ~3;

        private static uint Checksum(byte[] data)
        {
            uint sum = 0;
            for (var i = 0; i < data.Length; i += 4)
            {
                var word = 0u;
                for (var j = 0; j < 4; j++)
                    word = (word << 8) | (i + j < data.Length ? data[i + j] : (uint)0);

                sum += word;
            }

            return sum;
        }

        private static uint Tag(string name) => (uint)((name[0] << 24) | (name[1] << 16) | (name[2] << 8) | name[3]);
    }
}

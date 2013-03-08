﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
  internal class BsonBinaryWriter
  {
    private static readonly Encoding Encoding = Encoding.UTF8;

    private readonly BinaryWriter _writer;

    private byte[] _largeByteBuffer;
    private int _maxChars;

    public DateTimeKind DateTimeKindHandling { get; set; }

    public BsonBinaryWriter(Stream stream)
    {
      DateTimeKindHandling = DateTimeKind.Utc;
      _writer = new BinaryWriter(stream);
    }

    public void Flush()
    {
      _writer.Flush();
    }

    public void Close()
    {
      _writer.Close();
    }

    public void WriteToken(BsonToken t)
    {
      CalculateSize(t);
      WriteTokenInternal(t);
    }

    private void WriteTokenInternal(BsonToken t)
    {
      switch (t.Type)
      {
        case BsonType.Object:
          {
            BsonObject value = (BsonObject) t;
            _writer.Write(value.CalculatedSize);
            foreach (BsonProperty property in value)
            {
              _writer.Write((sbyte)property.Value.Type);
              WriteString((string)property.Name.Value, property.Name.ByteCount, null);
              WriteTokenInternal(property.Value);
            }
            _writer.Write((byte)0);
          }
          break;
        case BsonType.Array:
          {
            BsonArray value = (BsonArray) t;
            _writer.Write(value.CalculatedSize);
            int index = 0;
            foreach (BsonToken c in value)
            {
              _writer.Write((sbyte)c.Type);
              WriteString(index.ToString(CultureInfo.InvariantCulture), MathUtils.IntLength(index), null);
              WriteTokenInternal(c);
              index++;
            }
            _writer.Write((byte)0);
          }
          break;
        case BsonType.Integer:
          {
            BsonValue value = (BsonValue) t;
            _writer.Write(Convert.ToInt32(value.Value, CultureInfo.InvariantCulture));
          }
          break;
        case BsonType.Long:
          {
            BsonValue value = (BsonValue)t;
            _writer.Write(Convert.ToInt64(value.Value, CultureInfo.InvariantCulture));
          }
          break;
        case BsonType.Number:
          {
            BsonValue value = (BsonValue)t;
            _writer.Write(Convert.ToDouble(value.Value, CultureInfo.InvariantCulture));
          }
          break;
        case BsonType.String:
          {
            BsonString value = (BsonString)t;
            WriteString((string)value.Value, value.ByteCount, value.CalculatedSize - 4);
          }
          break;
        case BsonType.Boolean:
          {
            BsonValue value = (BsonValue)t;
            _writer.Write((bool)value.Value);
          }
          break;
        case BsonType.Null:
        case BsonType.Undefined:
          break;
        case BsonType.Date:
          {
            BsonValue value = (BsonValue)t;

            long ticks = 0;

            if (value.Value is DateTime)
            {
              DateTime dateTime = (DateTime) value.Value;
              if (DateTimeKindHandling == DateTimeKind.Utc)
                dateTime = dateTime.ToUniversalTime();
              else if (DateTimeKindHandling == DateTimeKind.Local)
                dateTime = dateTime.ToLocalTime();

              ticks = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTime, false);
            }
#if !PocketPC && !NET20
            else
            {
              DateTimeOffset dateTimeOffset = (DateTimeOffset) value.Value;
              ticks = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTimeOffset.UtcDateTime, dateTimeOffset.Offset);
            }
#endif

            _writer.Write(ticks);
          }
          break;
        case BsonType.Binary:
          {
            BsonValue value = (BsonValue)t;

            byte[] data = (byte[])value.Value;
            _writer.Write(data.Length);
            _writer.Write((byte)BsonBinaryType.Binary);
            _writer.Write(data);
          }
          break;
        case BsonType.Oid:
          {
            BsonValue value = (BsonValue)t;

            byte[] data = (byte[])value.Value;
            _writer.Write(data);
          }
          break;
        case BsonType.Regex:
          {
            BsonRegex value = (BsonRegex) t;

            WriteString((string)value.Pattern.Value, value.Pattern.ByteCount, null);
            WriteString((string)value.Options.Value, value.Options.ByteCount, null);
          }
          break;
        default:
          throw new ArgumentOutOfRangeException("t", "Unexpected token when writing BSON: {0}".FormatWith(CultureInfo.InvariantCulture, t.Type));
      }
    }

    private void WriteString(string s, int byteCount, int? calculatedlengthPrefix)
    {
      if (calculatedlengthPrefix != null)
        _writer.Write(calculatedlengthPrefix.Value);

      if (s != null)
      {
        if (_largeByteBuffer == null)
        {
          _largeByteBuffer = new byte[256];
          _maxChars = 256/Encoding.GetMaxByteCount(1);
        }
        if (byteCount <= 256)
        {
          Encoding.GetBytes(s, 0, s.Length, _largeByteBuffer, 0);
          _writer.Write(_largeByteBuffer, 0, byteCount);
        }
        else
        {
          int charCount;
          int totalCharsWritten = 0;
          for (int i = s.Length; i > 0; i -= charCount)
          {
            charCount = (i > _maxChars) ? _maxChars : i;
            int count = Encoding.GetBytes(s, totalCharsWritten, charCount, _largeByteBuffer, 0);
            _writer.Write(_largeByteBuffer, 0, count);
            totalCharsWritten += charCount;
          }
        }
      }

      _writer.Write((byte)0);
    }

    private int CalculateSize(int stringByteCount)
    {
      return stringByteCount + 1;
    }

    private int CalculateSizeWithLength(int stringByteCount, bool includeSize)
    {
      int baseSize = (includeSize)
        ? 5 // size bytes + terminator
        : 1; // terminator

      return baseSize + stringByteCount;
    }

    private int CalculateSize(BsonToken t)
    {
      switch (t.Type)
      {
        case BsonType.Object:
          {
            BsonObject value = (BsonObject) t;

            int bases = 4;
            foreach (BsonProperty p in value)
            {
              int size = 1;
              size += CalculateSize(p.Name);
              size += CalculateSize(p.Value);

              bases += size;
            }
            bases += 1;
            value.CalculatedSize = bases;
            return bases;
          }
        case BsonType.Array:
          {
            BsonArray value = (BsonArray)t;

            int size = 4;
            int index = 0;
            foreach (BsonToken c in value)
            {
              size += 1;
              size += CalculateSize(MathUtils.IntLength(index));
              size += CalculateSize(c);
              index++;
            }
            size += 1;
            value.CalculatedSize = size;

            return value.CalculatedSize;
          }
        case BsonType.Integer:
          return 4;
        case BsonType.Long:
          return 8;
        case BsonType.Number:
          return 8;
        case BsonType.String:
          {
            BsonString value = (BsonString)t;
            string s = (string) value.Value;
            value.ByteCount = (s != null) ? Encoding.GetByteCount(s) : 0;
            value.CalculatedSize = CalculateSizeWithLength(value.ByteCount, value.IncludeLength);

            return value.CalculatedSize;
          }
        case BsonType.Boolean:
          return 1;
        case BsonType.Null:
        case BsonType.Undefined:
          return 0;
        case BsonType.Date:
          return 8;
        case BsonType.Binary:
          {
            BsonValue value = (BsonValue) t;

            byte[] data = (byte[])value.Value;
            value.CalculatedSize = 4 + 1 + data.Length;

            return value.CalculatedSize;
          }
        case BsonType.Oid:
          return 12;
        case BsonType.Regex:
          {
            BsonRegex value = (BsonRegex) t;
            int size = 0;
            size += CalculateSize(value.Pattern);
            size += CalculateSize(value.Options);
            value.CalculatedSize = size;

            return value.CalculatedSize;
          }
        default:
          throw new ArgumentOutOfRangeException("t", "Unexpected token when writing BSON: {0}".FormatWith(CultureInfo.InvariantCulture, t.Type));
      }
    }
  }
}
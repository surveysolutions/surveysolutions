﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
#if !(SILVERLIGHT || MONODROID || MONOTOUCH)
using System.Data.SqlTypes;
#endif
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;

namespace Newtonsoft.Json.Converters
{
#if !SILVERLIGHT && !PocketPC && !NET20
  internal interface IBinary
  {
    byte[] ToArray();
  }
#endif

  /// <summary>
  /// Converts a binary value to and from a base 64 string value.
  /// </summary>
  public class BinaryConverter : JsonConverter
  {
#if !SILVERLIGHT && !PocketPC && !NET20
    private const string BinaryTypeName = "System.Data.Linq.Binary";
#endif

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      if (value == null)
      {
        writer.WriteNull();
        return;
      }

      byte[] data = GetByteArray(value);

      writer.WriteValue(data);
    }

    private byte[] GetByteArray(object value)
    {
#if !SILVERLIGHT && !PocketPC && !NET20 && !MONODROID
      if (value.GetType().AssignableToTypeName(BinaryTypeName))
      {
        IBinary binary = DynamicWrapper.CreateWrapper<IBinary>(value);
        return binary.ToArray();
      }
#endif
#if !(SILVERLIGHT || MONODROID || MONOTOUCH)
      if (value is SqlBinary)
        return ((SqlBinary) value).Value;
#endif
      throw new Exception("Unexpected value type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      Type t = (ReflectionUtils.IsNullableType(objectType))
        ? Nullable.GetUnderlyingType(objectType)
        : objectType;

      if (reader.TokenType == JsonToken.Null)
      {
        if (!ReflectionUtils.IsNullable(objectType))
          throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));

        return null;
      }

      byte[] data;

      if (reader.TokenType == JsonToken.StartArray)
      {
        data = ReadByteArray(reader);
      }
      else if (reader.TokenType == JsonToken.String)
      {
      // current token is already at base64 string
      // unable to call ReadAsBytes so do it the old fashion way
      string encodedData = reader.Value.ToString();
        data = Convert.FromBase64String(encodedData);
      }
      else
      {
        throw new Exception("Unexpected token parsing binary. Expected String or StartArray, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
      }

#if !SILVERLIGHT && !PocketPC && !NET20 && !MONODROID
      if (t.AssignableToTypeName(BinaryTypeName))
        return Activator.CreateInstance(t, data);
#endif
#if !(SILVERLIGHT || MONODROID || MONOTOUCH)
      if (t == typeof(SqlBinary))
        return new SqlBinary(data);
#endif
      throw new Exception("Unexpected object type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, objectType));
    }

    private byte[] ReadByteArray(JsonReader reader)
    {
      List<byte> byteList = new List<byte>();

      while (reader.Read())
      {
        switch (reader.TokenType)
        {
          case JsonToken.Integer:
            byteList.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
            break;
          case JsonToken.EndArray:
            return byteList.ToArray();
          case JsonToken.Comment:
            // skip
            break;
          default:
            throw new Exception("Unexpected token when reading bytes: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
        }
      }

      throw new Exception("Unexpected end when reading bytes.");
    }

    /// <summary>
    /// Determines whether this instance can convert the specified object type.
    /// </summary>
    /// <param name="objectType">Type of the object.</param>
    /// <returns>
    /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert(Type objectType)
    {
#if !SILVERLIGHT && !PocketPC && !NET20 && !MONODROID
      if (objectType.AssignableToTypeName(BinaryTypeName))
        return true;
#endif
#if !(SILVERLIGHT || MONODROID || MONOTOUCH)
      if (objectType == typeof(SqlBinary) || objectType == typeof(SqlBinary?))
        return true;
#endif
      return false;
    }
  }
}
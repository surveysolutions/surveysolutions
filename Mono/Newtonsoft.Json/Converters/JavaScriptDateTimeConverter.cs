﻿using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts a <see cref="DateTime"/> to and from a JavaScript date constructor (e.g. new Date(52231943)).
  /// </summary>
  public class JavaScriptDateTimeConverter : DateTimeConverterBase
  {
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      long ticks;

      if (value is DateTime)
      {
        DateTime dateTime = (DateTime)value;
        DateTime utcDateTime = dateTime.ToUniversalTime();
        ticks = JsonConvert.ConvertDateTimeToJavaScriptTicks(utcDateTime);
      }
#if !PocketPC && !NET20
      else if (value is DateTimeOffset)
      {
        DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
        DateTimeOffset utcDateTimeOffset = dateTimeOffset.ToUniversalTime();
        ticks = JsonConvert.ConvertDateTimeToJavaScriptTicks(utcDateTimeOffset.UtcDateTime);
      }
#endif
      else
      {
        throw new Exception("Expected date object value.");
      }

      writer.WriteStartConstructor("Date");
      writer.WriteValue(ticks);
      writer.WriteEndConstructor();
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      Type t = (ReflectionUtils.IsNullableType(objectType))
        ? Nullable.GetUnderlyingType(objectType)
        : objectType;

      if (reader.TokenType == JsonToken.Null)
      {
        if (!ReflectionUtils.IsNullableType(objectType))
          throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));

        return null;
      }

      if (reader.TokenType != JsonToken.StartConstructor || string.Compare(reader.Value.ToString(), "Date", StringComparison.Ordinal) != 0)
        throw new Exception("Unexpected token or value when parsing date. Token: {0}, Value: {1}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType, reader.Value));

      reader.Read();

      if (reader.TokenType != JsonToken.Integer)
        throw new Exception("Unexpected token parsing date. Expected Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

      long ticks = (long)reader.Value;

      DateTime d = JsonConvert.ConvertJavaScriptTicksToDateTime(ticks);

      reader.Read();

      if (reader.TokenType != JsonToken.EndConstructor)
        throw new Exception("Unexpected token parsing date. Expected EndConstructor, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

#if !PocketPC && !NET20
      if (t == typeof(DateTimeOffset))
        return new DateTimeOffset(d);
#endif

      return d;
    }
  }
}
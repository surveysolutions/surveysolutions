﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Reflection;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts a <see cref="KeyValuePair{TKey,TValue}"/> to and from JSON.
  /// </summary>
  public class KeyValuePairConverter : JsonConverter
  {
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value.</param>
    /// <param name="serializer">The calling serializer.</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      Type t = value.GetType();
      PropertyInfo keyProperty = t.GetProperty("Key");
      PropertyInfo valueProperty = t.GetProperty("Value");

      writer.WriteStartObject();
      writer.WritePropertyName("Key");
      serializer.Serialize(writer, ReflectionUtils.GetMemberValue(keyProperty, value));
      writer.WritePropertyName("Value");
      serializer.Serialize(writer, ReflectionUtils.GetMemberValue(valueProperty, value));
      writer.WriteEndObject();
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
      IList<Type> genericArguments = objectType.GetGenericArguments();
      Type keyType = genericArguments[0];
      Type valueType = genericArguments[1];

      reader.Read();
      reader.Read();
      object key = serializer.Deserialize(reader, keyType);
      reader.Read();
      reader.Read();
      object value = serializer.Deserialize(reader, valueType);
      reader.Read();

      return ReflectionUtils.CreateInstance(objectType, key, value);
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
      if (objectType.IsValueType && objectType.IsGenericType)
        return (objectType.GetGenericTypeDefinition() == typeof (KeyValuePair<,>));

      return false;
    }
  }
}
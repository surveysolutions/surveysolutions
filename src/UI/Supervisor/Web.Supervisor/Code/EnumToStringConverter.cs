using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Web.Supervisor.Code
{
    public class EnumToStringConverter : Newtonsoft.Json.Converters.StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Action)
            {
                writer.WriteValue(Enum.GetName(typeof(Action), (Action)value));// or something else
                return;
            }

            base.WriteJson(writer, value, serializer);
        }
    }
}
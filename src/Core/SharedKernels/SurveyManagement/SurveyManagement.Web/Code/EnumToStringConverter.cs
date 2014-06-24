using System;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
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
using System;
using Newtonsoft.Json.Linq;
using NJsonSchema.Converters;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class QuestionnaireEntityJsonConverter : JsonInheritanceConverter
    {
        public QuestionnaireEntityJsonConverter(string name) : base(name)
        {
        }

        public override string GetDiscriminatorValue(Type type)
        {
            return type.Name;
        }

        protected override Type GetDiscriminatorType(JObject jObject, Type objectType, string discriminatorValue)
        {
            return base.GetDiscriminatorType(jObject, objectType, discriminatorValue);
        }
    }
}
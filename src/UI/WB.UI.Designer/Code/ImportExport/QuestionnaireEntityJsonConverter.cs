using System;
using Newtonsoft.Json.Linq;
using NJsonSchema.Converters;
using WB.UI.Designer.Code.ImportExport.Models.Question;

namespace WB.UI.Designer.Code.ImportExport
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
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WB.UI.Headquarters.Code.SwaggerCustomization
{
    public class XmsEnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                XmsEnumExtensionApplicator.Apply(schema.Extensions, context.Type);
            };
        }
    }
}
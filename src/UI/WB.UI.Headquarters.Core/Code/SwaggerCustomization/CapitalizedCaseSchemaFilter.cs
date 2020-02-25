using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WB.UI.Headquarters.Code.SwaggerCustomization
{
    public class CapitalizedCaseSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null) return;
            if (schema.Properties.Count == 0) return;

            var keys = schema.Properties.Keys;
            var newProperties = new Dictionary<string, OpenApiSchema>();

            foreach (var key in keys)
            {
                newProperties[key.Capitalize()] = schema.Properties[key];
            }

            schema.Properties = newProperties;
        }
    }
}
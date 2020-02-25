using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;

namespace WB.UI.Headquarters.Code.SwaggerCustomization
{
    public static class XmsEnumExtensionApplicator
    {
        public static void Apply(IDictionary<string, IOpenApiExtension> extensions, Type enumType)
        {
            var name = enumType.GetTypeInfo().IsGenericType && enumType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? enumType.GetGenericArguments()[0].Name
                : enumType.Name;

            extensions["x-ms-enum"] = new OpenApiObject
            {
                ["name"] = new OpenApiString(name),
                ["modelAsString"] = new OpenApiBoolean(false)
            };
        }
    }
}
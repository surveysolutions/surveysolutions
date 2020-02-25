using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WB.UI.Headquarters.Code.SwaggerCustomization
{
  
    public class XmsEnumParameterFilter : IParameterFilter
    {
        private static IEnumerable<(Type Type, string Name)> GetEnumParameters(OperationFilterContext context)
        {
            return context
                .ApiDescription
                .ParameterDescriptions
                .Where(x => x.Type != null)
                .Select(x => (Type: TryGetEnumType(x), Name: x.Name))
                .Where(x => x.Type != null);
        }

        private static Type TryGetEnumType(ApiParameterDescription parameter)
        {
            if (parameter.Type == null)
            {
                return null;
            }

            if (parameter.Type.GetTypeInfo().IsEnum)
            {
                return parameter.Type;
            }

            var nullableUnderlyingType = Nullable.GetUnderlyingType(parameter.Type);

            return nullableUnderlyingType?.GetTypeInfo().IsEnum == true
                ? nullableUnderlyingType 
                : null;
        }

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var enumType = TryGetEnumType(context.ApiParameterDescription);

            if (enumType != null)
            {
                XmsEnumExtensionApplicator.Apply(parameter.Extensions, enumType);
            }
        }
    }
}

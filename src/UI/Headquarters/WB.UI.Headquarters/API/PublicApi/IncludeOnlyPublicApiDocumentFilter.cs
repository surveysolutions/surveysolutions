using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace WB.UI.Headquarters.API.PublicApi
{
    public class IncludeOnlyPublicApiDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            IDictionary<string, PathItem> pathsToKeep = new SortedDictionary<string, PathItem>();
            foreach (ApiDescription apiDescription in apiExplorer.ApiDescriptions)
            {
                var controllerTypeNamespace = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType.Namespace;

                if (controllerTypeNamespace.Contains("PublicApi"))
                {
                    var pathToRemove = $"/{apiDescription.Route.RouteTemplate.TrimEnd('/')}";
                    pathsToKeep[pathToRemove] = swaggerDoc.paths[pathToRemove];
                }
            }

            swaggerDoc.paths = pathsToKeep;
        }
    }
}

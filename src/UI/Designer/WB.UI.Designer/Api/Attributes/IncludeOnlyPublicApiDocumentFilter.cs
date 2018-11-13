using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using WB.UI.Designer.Api.Designer;
using System.Linq;

namespace WB.UI.Designer.Api.Attributes
{
    public class IncludeOnlyPublicApiDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            IDictionary<string, PathItem> pathsToKeep = new Dictionary<string, PathItem>();
            var filters = new string[] {"Api.Portal", nameof(ClassificationsController)};
            foreach (ApiDescription apiDescription in apiExplorer.ApiDescriptions)
            {
                var controllerTypeNamespace = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType.FullName;

                if (controllerTypeNamespace == null) continue;
                if (!filters.Any(x => controllerTypeNamespace.Contains(x))) continue;

                var pathToRemove = $"/{apiDescription.Route.RouteTemplate.TrimEnd('/')}";
                pathsToKeep[pathToRemove] = swaggerDoc.paths[pathToRemove];
            }

            swaggerDoc.paths = pathsToKeep;
        }
    }
}

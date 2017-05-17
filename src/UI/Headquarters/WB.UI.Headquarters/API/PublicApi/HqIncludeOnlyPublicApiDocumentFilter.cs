using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace WB.UI.Headquarters.API.PublicApi
{
    public class HqIncludeOnlyPublicApiDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (ApiDescription apiDescription in apiExplorer.ApiDescriptions)
            {
                var controllerTypeNamespace = apiDescription.ActionDescriptor.ControllerDescriptor.ControllerType.Namespace;
                if (!controllerTypeNamespace.Contains("PublicApi"))
                {
                    var pathToRemove = $"/{apiDescription.Route.RouteTemplate.TrimEnd('/')}";
                    swaggerDoc.paths.Remove(pathToRemove);
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using WB.UI.Designer.Api.Designer;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

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
            IDictionary<string, Schema> definitionsToKeep = new Dictionary<string, Schema>();

            

            var refsToProcess = new Queue<string>();
            foreach (PathItem path in swaggerDoc.paths.Values)
            {
                var refs = new Operation[] {path.get, path.put, path.post, path.delete, path.options, path.head, path.patch}
                    .Where(x => x != null)
                    .SelectMany(x => x.responses.Values.Select(r => GetReference(r.schema)))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();
                
                refs.ForEach(x => refsToProcess.Enqueue(x));
            }

            while (refsToProcess.Count > 0)
            {
                var @ref = refsToProcess.Dequeue();
                var schema = swaggerDoc.definitions[@ref];
                definitionsToKeep[@ref] = schema;
                schema.properties.Values.Select(GetReference).Where(x => !string.IsNullOrWhiteSpace(x)).ForEach(x => refsToProcess.Enqueue(x));
            }
            
            swaggerDoc.definitions = definitionsToKeep;

            string GetReference(Schema schema)
            {
                return (schema?.@ref ?? schema?.items?.@ref ?? "").Replace("#/definitions/", "");
            }
        }
    }
}

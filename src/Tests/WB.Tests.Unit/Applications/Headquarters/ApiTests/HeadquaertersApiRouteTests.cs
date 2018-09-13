using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters;
using WB.UI.Headquarters.Filters;

namespace WB.Tests.Unit.Applications.Headquarters.ApiTests
{
    [TestFixture]
    public class HeadquaertersApiRouteTests
    {
        public const string DataCollectionApiNamespace = "WB.UI.Headquarters.API.DataCollection";

        [Test]
        public void all_public_methods_in_api_controllers_should_have_any_http_attribute()
        {
            var allPublicMethods = GetAllPublicApiMethods();

            var methodsWithoutHttpAttribute = allPublicMethods.Where(m =>
                m.GetCustomAttribute(typeof(HttpPostAttribute)) == null
                && m.GetCustomAttribute(typeof(HttpGetAttribute)) == null
                && m.GetCustomAttribute(typeof(HttpDeleteAttribute)) == null
            ).ToList();

            CollectionAssert.IsEmpty(methodsWithoutHttpAttribute);
        }

        [Test]
        public void should_all_public_methods_in_api_for_data_collection_will_be_registered_in_api_routers()
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            var typedRoutes = TypedDirectRouteProvider.Routes.SelectMany(kv => kv.Value.Values);
            var actionsInRouters = typedRoutes.Select(r => $"{r.ControllerType.FullName}->{r.ActionMember.ToString()}")
                .OrderBy(v => v).ToList();

            var allPublicMethodsInApiControllers = GetAllPublicApiMethods();
            var actionsInApiClasses = allPublicMethodsInApiControllers.Select(m => $"{m.DeclaringType.FullName}->{m.ToString()}")
                .OrderBy(v => v).ToList();

            CollectionAssert.AreEqual(actionsInRouters, actionsInApiClasses);
        }

        [Test]
        public void should_all_public_methods_in_base_classes_be_overraded_in_api_controllers()
        {
            var assembly = Assembly.GetAssembly(typeof(WebApiConfig));
            var allApiDataCollectionControllers = assembly.GetTypes().Where(t =>
                t.IsSubclassOf(typeof(ApiController))
                && t.Namespace.StartsWith(DataCollectionApiNamespace)
            ).ToList();
            var apiAbstractControllers = allApiDataCollectionControllers.Where(t =>
                t.IsAbstract
            ).ToList();
            var apiNestedControllers = allApiDataCollectionControllers.Where(t =>
                !t.IsAbstract
            ).ToList();
            var allPublicMethodsInAbstractControllers = apiAbstractControllers.SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && !m.IsFamily && m.IsPublic && m.Name == "Census")
            ).ToList();
            var allPublicMethodsInNestedControllers = apiNestedControllers.SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && !m.IsFamily && m.IsPublic)
            ).ToList();

            var mapControllerMethods = new Dictionary<Type, HashSet<string>>();
            foreach (var method in allPublicMethodsInNestedControllers)
            {
                if (!mapControllerMethods.ContainsKey(method.DeclaringType))
                    mapControllerMethods.Add(method.DeclaringType, new HashSet<string>());

                mapControllerMethods[method.DeclaringType].Add(method.Name);
            }

            var methodWithoutOverride = allPublicMethodsInAbstractControllers.SelectMany(m =>
            {
                var allNestedControllers = mapControllerMethods.Where(kv => kv.Key.IsSubclassOf(m.DeclaringType));
                return allNestedControllers.Where(kv => !kv.Value.Contains(m.Name))
                    .Select(kv => $"Controller {kv.Key.Name} doesn't have method {m.Name}");
            }).ToList();

            CollectionAssert.IsEmpty(methodWithoutOverride);
        }

        private static List<MethodInfo> GetAllPublicApiMethods()
        {
            var assembly = Assembly.GetAssembly(typeof(WebApiConfig));
            var apiControllers = assembly.GetTypes().Where(t => 
                t.IsSubclassOf(typeof(ApiController))
                && t.Namespace.StartsWith(DataCollectionApiNamespace)
                && !t.IsAbstract
            );
            var allPublicMethods = apiControllers.SelectMany(controller => controller
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && !m.IsFamily && m.IsPublic)
            ).ToList();
            return allPublicMethods;
        }

    }
}

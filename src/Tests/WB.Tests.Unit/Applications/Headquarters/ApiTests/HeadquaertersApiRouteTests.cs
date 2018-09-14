using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
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

        [Test]
        public void all_urls_used_in_Interviewer_SynchronizationService_should_be_declared_in_routes()
        {
            var restService = new DummyRestSeviceForCollectUrls();

            var checkVersionUrl = "CheckVersionUrl";

            var interviewerSynchronizationService = new WB.Core.BoundedContexts.Interviewer.Implementation.Services.SynchronizationService(
                Mock.Of<IPrincipal>(),
                restService,
                Mock.Of<IInterviewerSettings>(m => m.GetSupportedQuestionnaireContentVersion() == new Version(1, 1, 1)
                                                   && m.GetDeviceId() == "DeviceId"),
                Mock.Of<IInterviewerSyncProtocolVersionProvider>(),
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICheckVersionUriProvider>(m => m.CheckVersionUrl == checkVersionUrl),
                Mock.Of<ILogger>()
            );

            all_urls_used_in_SynchronizationService_should_be_declared_in_routes(
                interviewerSynchronizationService,
                restService,
                checkVersionUrl);
        }

        [Test]
        public void all_urls_used_in_Supervisor_SynchronizationService_should_be_declared_in_routes()
        {
            var restService = new DummyRestSeviceForCollectUrls();

            var checkVersionUrl = "CheckVersionUrl";

            var supervisorSynchronizationService = new WB.Core.BoundedContexts.Supervisor.Services.Implementation.SynchronizationService(
                Mock.Of<IPrincipal>(),
                restService,
                Mock.Of<ISupervisorSettings>(m => m.GetSupportedQuestionnaireContentVersion() == new Version(1, 1, 1)
                                                   && m.GetDeviceId() == "DeviceId"),
                Mock.Of<ISupervisorSyncProtocolVersionProvider>(),
                Mock.Of<IFileSystemAccessor>(),
                Mock.Of<ICheckVersionUriProvider>(m => m.CheckVersionUrl == checkVersionUrl),
                Mock.Of<ILogger>()
            );

            all_urls_used_in_SynchronizationService_should_be_declared_in_routes(supervisorSynchronizationService,
                restService,
                checkVersionUrl);
        }


        private void all_urls_used_in_SynchronizationService_should_be_declared_in_routes(
            EnumeratorSynchronizationService synchronizationService,
            DummyRestSeviceForCollectUrls restService,
            string checkVersionUrl)
        {
            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            var typedRoutes = TypedDirectRouteProvider.Routes.SelectMany(kv => kv.Value.Values);
            var routeTemplates = typedRoutes.OrderBy(r => r.Template)
                .Select(r => GenerateRegexFromTemplate(r.Template))
                .ToList();

            var allPublicMethods = synchronizationService.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.IsPublic);

            foreach (var method in allPublicMethods)
            {
                try
                {
                    var parameters = method.GetParameters();
                    var objectParameters = parameters.Select(p =>
                    {
                        if (p.ParameterType == typeof(string))
                            return "test";
                        if (p.ParameterType == typeof(byte[]))
                            return new byte[0];
                        if (p.ParameterType == typeof(IProgress<TransferProgress>))
                            return new DummyTransferProgress();

                        return Activator.CreateInstance(p.ParameterType);
                    }).ToArray();
                    method.Invoke(synchronizationService, objectParameters);
                }
                catch(SynchronizationException ex) when (ex.Type == SynchronizationExceptionType.InvalidUrl)
                {
                    if (method.Name == "CanSynchronizeAsync")
                        return; /* ignore */

                    throw;
                }
            }

            var urlWithoutRoute = restService.Urls.Where(url => 
                !url.StartsWith(checkVersionUrl)
                && !routeTemplates.Any(regex => regex.IsMatch(url))
            ).ToList();

            CollectionAssert.IsNotEmpty(restService.Urls);
            CollectionAssert.IsEmpty(urlWithoutRoute);
        }

        private static readonly Regex RegexReplaceTemplateVariables = new Regex(@"{[\w:]+}", RegexOptions.Singleline | RegexOptions.Compiled);

        private Regex GenerateRegexFromTemplate(string template)
        {
            var regexTemplate = RegexReplaceTemplateVariables.Replace(template, ".*");
            return new Regex($"^{regexTemplate}$", RegexOptions.Singleline);
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

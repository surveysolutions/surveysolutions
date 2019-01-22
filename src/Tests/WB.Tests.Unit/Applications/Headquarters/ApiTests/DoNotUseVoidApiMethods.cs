using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using NUnit.Framework;
using WB.UI.Headquarters.API;

namespace WB.Tests.Unit.Applications.Headquarters.ApiTests
{
    public class DoNotUseVoidApiMethods
    {
        [Test]
        public void methods_produce_web_requests_that_are_not_returning_exit_codes_and_are_invalid_REST_methods()
        {
            var controllersAssembly = typeof(AdminSettingsController).Assembly;
            var apiControllerTypes = controllersAssembly.GetExportedTypes()
                .Where(x => x.IsSubclassOf(typeof(ApiController)) && !x.IsAbstract);

            List<MethodInfo> voidApiMethods = new List<MethodInfo>();
            foreach (var apiController in apiControllerTypes)
            {
                var methods = apiController.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                           .Where(m => !m.IsSpecialName && !m.IsFamily);

                foreach (var methodInfo in methods)
                {
                    if (methodInfo.ReturnType == typeof(void))
                    {
                        voidApiMethods.Add(methodInfo);
                    }
                }
            }

            Assert.That(voidApiMethods, Is.Empty, () =>
            {
                string result = "Expected no void api methods, but was " + Environment.NewLine;

                return result + string.Join(Environment.NewLine,
                    voidApiMethods.Select(x => $"Controller {x.DeclaringType}.{x.Name}"));
            });

        }
    }
}

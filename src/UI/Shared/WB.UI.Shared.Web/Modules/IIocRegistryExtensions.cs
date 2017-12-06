using System;
using System.Web.Http.Filters;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public static class IIocRegistryExtensions
    {
        public static void BindFilter<T>(this IIocRegistry registry, System.Web.Mvc.FilterScope filterScope, int? order)
        {
            var moduleAdapter = (NinjectModuleAdapter) registry;
            moduleAdapter.BindMvcFilter<T>(filterScope, order);
        }

        public static void BindFilterWhenActionMethodHasNoAttribute<T, TAttribute>(this IIocRegistry registry, System.Web.Mvc.FilterScope filterScope, int? order)
        {
            var moduleAdapter = (NinjectModuleAdapter)registry;
            moduleAdapter.BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(filterScope, order);
        }

        public static void BindHttpFilter<T>(this IIocRegistry registry, System.Web.Http.Filters.FilterScope filterScope, int? order) where T : IFilter
        {
            var moduleAdapter = (NinjectModuleAdapter)registry;
            moduleAdapter.BindHttpFilter<T>(filterScope, order);
        }

        public static void BindHttpFilterWhenActionMethodHasNoAttribute<T, TAttribute>(this IIocRegistry registry, System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter
        {
            var moduleAdapter = (NinjectModuleAdapter)registry;
            moduleAdapter.BindHttpFilterWhenActionMethodHasNoAttribute<T>(filterScope, order, typeof(TAttribute));
        }

    }
}
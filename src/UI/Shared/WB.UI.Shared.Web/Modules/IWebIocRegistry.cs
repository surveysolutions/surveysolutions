using System.Web.Http.Filters;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Modules
{
    public interface IWebIocRegistry : IIocRegistry
    {
        void BindMvcFilter<T>(System.Web.Mvc.FilterScope filterScope, int? order);

        void BindMvcFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Mvc.FilterScope filterScope, int? order);

        void BindHttpFilter<T>(System.Web.Http.Filters.FilterScope filterScope, int? order) where T : IFilter;

        void BindHttpFilterWhenActionMethodHasNoAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter;

        void BindHttpFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, int? order = null) where T : IFilter;

        void BindHttpFilterWhenControllerHasAttribute<T, TAttribute>(System.Web.Http.Filters.FilterScope filterScope, ConstructorArgument constructorArgument) where T : IFilter;
    }
}

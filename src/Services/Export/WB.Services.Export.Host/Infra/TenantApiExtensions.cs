using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Host.Infra
{
    public class TenantEntityBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.Metadata.ModelType == typeof(TenantInfo))
                return (IModelBinder)new TenantEntityBinder();
            return (IModelBinder)null;
        }
    }

    public class TenantEntityBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var headers = bindingContext.HttpContext.Request.Headers;
            var auth = headers["Authorization"];
            if (!auth.Any() || !auth.ToString().StartsWith("Bearer "))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            string baseUrl = headers["Referer"];
            string tenantId = headers["Authorization"].ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
            string name = headers["x-tenant-name"];

            TenantInfo tenantInfo = new TenantInfo(baseUrl, tenantId, name);
            bindingContext.Result = ModelBindingResult.Success(tenantInfo);
            return Task.CompletedTask;
        }
    }
}

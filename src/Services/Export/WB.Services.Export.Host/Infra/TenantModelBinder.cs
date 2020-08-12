using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WB.Services.Export.Host.Infra
{
    public class TenantModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var tenantInfo = bindingContext.HttpContext.Request.GetTenantInfo();
            if (tenantInfo == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
            bindingContext.Result = ModelBindingResult.Success(tenantInfo);
            return Task.CompletedTask;
        }
    }
}


using System;
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
                return new TenantEntityBinder();

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Events
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
            => items.ToDictionary(item => item.Key, item => item.Value);
    }

    public static class TenatnContextHelpers
    {
        public static void PropagateTenantContext(this IServiceScope scope, ITenantContext context)
        {
            scope.ServiceProvider.GetService<ITenantContext>().Tenant = context.Tenant;
        }
    }
}

using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.UI.Headquarters.Services.Impl
{
    public class DesignerUserCredentials : IDesignerUserCredentials
    {
        private readonly IHttpContextAccessor contextAccessor;

        public DesignerUserCredentials(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        private static readonly AsyncLocal<RestCredentials> taskValue = new AsyncLocal<RestCredentials>();

        public virtual RestCredentials Get()
        {
            if (taskValue.Value != null) 
                return taskValue.Value;
            
            var restCredentials = contextAccessor.HttpContext?.Session.GetString("designerCredentials");
            if (restCredentials == null)
                return null;
            return JsonConvert.DeserializeObject<RestCredentials>(restCredentials);
        }

        public void Set(RestCredentials credentials)
        {
            contextAccessor.HttpContext.Session.SetString("designerCredentials", JsonConvert.SerializeObject(credentials));
        }

        public void SetTaskCredentials(RestCredentials restCredentials)
        {
            taskValue.Value = restCredentials;
        }
    }
}

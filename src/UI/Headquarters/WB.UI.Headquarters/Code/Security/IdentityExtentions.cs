using System;
using System.Security.Principal;
namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.Security
{
    public static class IdentityExtentions
    {
        public static bool IsObserver(this IIdentity identity)
        {
            var customIdentity = identity as CustomIdentity;
            
            if (customIdentity == null)
                throw new Exception("Incorrect type of instance of IIdentity");

            return customIdentity.IsObserver;
        }

        public static string GetObserverName(this IIdentity identity)
        {
            var customIdentity = identity as CustomIdentity;

            return customIdentity != null ? customIdentity.ObserverName : string.Empty;
        }
    }
}

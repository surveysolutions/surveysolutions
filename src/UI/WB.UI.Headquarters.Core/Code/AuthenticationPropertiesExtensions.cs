using System;
using Microsoft.AspNetCore.Authentication;

namespace WB.UI.Headquarters.Code
{
    public static class AuthenticationPropertiesExtensions
    {
        public static AuthenticationProperties WithReason(this AuthenticationProperties props, ForbidReason? reason)
        {
            props.Items[".reason"] = reason?.ToString();
            return props;
        }

        public static bool TryGetForbidReason(this AuthenticationProperties props, out ForbidReason forbidReason)
        {
            forbidReason = ForbidReason.None;

            if (!props.Items.TryGetValue(".reason", out var reason)) return false;
            if (!Enum.TryParse<ForbidReason>(reason, out var result)) return false;
            forbidReason = result;
            return true;
        }
    }
}

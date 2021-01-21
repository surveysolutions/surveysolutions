using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class AllowPrimaryWorkspaceFallbackAttribute : Attribute, IFilterMetadata
    {
        
    }
}

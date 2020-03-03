using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Shared.Web.Attributes
{
    public class NoTransactionAttribute : Attribute, IFilterMetadata
    {
        
    }
}

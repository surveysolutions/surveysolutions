using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.ValueObjects;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface INavigationService
    {
        void NavigateTo(CapiPages navigateToPage, Dictionary<string, string> pageParameters, bool clearHistory = false);
    }
}

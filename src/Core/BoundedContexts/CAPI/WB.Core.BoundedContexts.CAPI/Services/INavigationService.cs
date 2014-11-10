using System.Collections.Specialized;
using WB.Core.BoundedContexts.Capi.ValueObjects;

namespace WB.Core.BoundedContexts.Capi.Services
{
    public interface INavigationService
    {
        void NavigateTo(CapiPages navigateToPage, NameValueCollection pageParameters);
    }
}

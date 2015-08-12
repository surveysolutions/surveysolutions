using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IFriendlyErrorMessageService
    {
        string GetFriendlyErrorMessageByRestException(RestException ex);
    }
}
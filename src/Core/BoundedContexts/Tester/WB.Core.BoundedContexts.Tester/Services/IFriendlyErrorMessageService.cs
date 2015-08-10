using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IFriendlyErrorMessageService
    {
        string GetFriendlyErrorMessageByRestException(RestException ex);
    }
}
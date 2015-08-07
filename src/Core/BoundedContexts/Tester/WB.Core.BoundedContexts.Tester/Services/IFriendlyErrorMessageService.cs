using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IFriendlyErrorMessageService
    {
        string GetFriendlyErrorMessageByRestException(RestException ex);
    }
}
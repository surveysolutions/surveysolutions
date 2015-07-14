using WB.Core.BoundedContexts.Tester.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IFriendlyMessageService
    {
        string GetFriendlyErrorMessageByRestException(RestException ex);
    }
}
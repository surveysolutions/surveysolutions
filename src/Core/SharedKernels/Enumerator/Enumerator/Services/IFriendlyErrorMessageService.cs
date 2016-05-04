using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IFriendlyErrorMessageService
    {
        string GetFriendlyErrorMessageByRestException(RestException ex);
    }
}
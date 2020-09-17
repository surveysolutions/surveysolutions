using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IFriendlyErrorMessageService
    {
        string GetFriendlyErrorMessageByRestException(RestException ex);
    }
}
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorBinaryHandler : IHandleCommunicationMessage
    {
        private readonly IPlainStorage<CompanyLogo> logoStorage;

        public SupervisorBinaryHandler(IPlainStorage<CompanyLogo> logoStorage)
        {
            this.logoStorage = logoStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetCompanyLogoRequest, GetCompanyLogoResponse>(GetCompanyLogo);
        }
        
        public Task<GetCompanyLogoResponse> GetCompanyLogo(GetCompanyLogoRequest request)
        {
            var existingLogo = logoStorage.GetById(CompanyLogo.StorageKey);
            if (existingLogo == null)
            {
                return Task.FromResult(new GetCompanyLogoResponse
                {
                    LogoInfo = new CompanyLogoInfo
                    {
                        HasCustomLogo = false,
                        LogoNeedsToBeUpdated = !string.IsNullOrEmpty(request.Etag)
                    }
                });
            }

            var needUpdate = existingLogo.ETag != request.Etag;

            return Task.FromResult(new GetCompanyLogoResponse
            {
                LogoInfo = new CompanyLogoInfo
                {
                    Etag = existingLogo.ETag,
                    Logo = needUpdate ? existingLogo.File : null,
                    LogoNeedsToBeUpdated = needUpdate,
                    HasCustomLogo = true
                }
            });
        }
    }
}

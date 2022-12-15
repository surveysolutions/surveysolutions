using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorInterviewerSettingsHandler : IHandleCommunicationMessage
    {
        private readonly ISupervisorSettings supervisorSettings;

        public SupervisorInterviewerSettingsHandler(
            ISupervisorSettings supervisorSettings)
        {
            this.supervisorSettings = supervisorSettings;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<RemoteTabletSettingsRequest, RemoteTabletSettingsResponse>(GetInterviewerSettings);
        }

        public Task<RemoteTabletSettingsResponse> GetInterviewerSettings(RemoteTabletSettingsRequest request)
        {
            var geographyQuestionAccuracyInMeters = supervisorSettings.GeographyQuestionAccuracyInMeters;
            var geographyQuestionPeriodInSeconds = supervisorSettings.GeographyQuestionPeriodInSeconds;
            var webInterviewUriTemplate = supervisorSettings.WebInterviewUriTemplate;
            var notificationsEnabled = supervisorSettings.NotificationsEnabled;

            var response = new RemoteTabletSettingsResponse
            {
                Settings = new RemoteTabletSettingsApiView
                {
                    PartialSynchronizationEnabled = false,
                    GeographyQuestionAccuracyInMeters = geographyQuestionAccuracyInMeters,
                    GeographyQuestionPeriodInSeconds = geographyQuestionPeriodInSeconds,
                    WebInterviewUrlTemplate = webInterviewUriTemplate,
                    NotificationsEnabled = notificationsEnabled,
                }
            };
            return Task.FromResult(response);
        }
    }
}

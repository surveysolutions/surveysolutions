using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class UploadCalendarEvents : SynchronizationStep
    {
        private readonly IEnumeratorEventStorage eventStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IJsonAllTypesSerializer synchronizationSerializer;
        
        public UploadCalendarEvents(int sortOrder, ISynchronizationService synchronizationService, 
            ILogger logger, IEnumeratorEventStorage eventStorage, 
            IPlainStorage<InterviewView> interviewViewRepository, 
            IJsonAllTypesSerializer synchronizationSerializer) 
            : base(sortOrder, synchronizationService, logger)
        {
            this.eventStorage = eventStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.synchronizationSerializer = synchronizationSerializer;
        }

        public override async Task ExecuteAsync()
        {
            var allInterviewsWithCalendarEvents = interviewViewRepository
                .LoadAll()
                .Where(x => x.CalendarEventId != null)
                .ToList();

            var transferProgress = Context.Progress.AsTransferReport();
            
            foreach (var interview in allInterviewsWithCalendarEvents)
            {
                var eventsToSend =  this.eventStorage.Read(interview.CalendarEventId.Value, 0)
                    .ToReadOnlyCollection();
                
                var package = new CalendarEventPackageApiView()
                {
                    CalendarEventId = interview.CalendarEventId.Value,
                    Events = this.synchronizationSerializer.Serialize(eventsToSend),
                    MetaInfo = new CalendarEventMetaInfo()
                    {
                        ResponsibleId   = interview.ResponsibleId,
                        LastUpdateDateTime = eventsToSend.Last().EventTimeStamp
                    }
                };

                await this.synchronizationService.UploadCalendarEventAsync(
                    interview.CalendarEventId.Value,
                    package,
                    transferProgress,
                    Context.CancellationToken);
            }
        }
    }
}

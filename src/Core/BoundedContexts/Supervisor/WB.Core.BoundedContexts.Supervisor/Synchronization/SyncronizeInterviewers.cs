using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization
{
    public class SyncronizeInterviewers : SynchronizationStep
    {
        private readonly ISupervisorSynchronizationService supervisorSynchronization;
        private readonly IPlainStorage<InterviewerDocument> interviewerViewRepository;

        public SyncronizeInterviewers(int sortOrder, ILogger logger,
            ISupervisorSynchronizationService supervisorSynchronization,
            IPlainStorage<InterviewerDocument> interviewerViewRepository) : base(sortOrder, supervisorSynchronization,
            logger)
        {
            this.supervisorSynchronization = supervisorSynchronization;
            this.interviewerViewRepository = interviewerViewRepository;
        }

        public override async Task ExecuteAsync()
        {
            var processedInterviewersCount = 0;
            Context.Progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_Interviewers,
                Statistics = Context.Statistics,
                Status = SynchronizationStatus.Download
            });

            var remoteInterviewers = await this.supervisorSynchronization.GetInterviewersAsync(Context.CancellationToken);
            var localInterviewers = this.interviewerViewRepository.LoadAll();

            var interviewersToRemove = localInterviewers.Select(x => x.InterviewerId)
                .Except(remoteInterviewers.Select(x => x.Id)).ToList();
            foreach (var interviewerId in interviewersToRemove)
            {
                this.interviewerViewRepository.Remove(interviewerId.FormatGuid());
            }

            processedInterviewersCount += interviewersToRemove.Count;
            var localInterviewersLookup = this.interviewerViewRepository.LoadAll().ToLookup(x => x.InterviewerId);

            foreach (var interviewer in remoteInterviewers)
            {
                var local = localInterviewersLookup[interviewer.Id].FirstOrDefault();
                if (local == null)
                {
                    local = new InterviewerDocument
                    {
                        Id = interviewer.Id.FormatGuid(),
                        InterviewerId = interviewer.Id,
                        CreationDate = interviewer.CreationDate,
                        Email = interviewer.Email,
                        PasswordHash = interviewer.PasswordHash,
                        PhoneNumber = interviewer.PhoneNumber,
                        UserName = interviewer.UserName,
                        FullaName = interviewer.FullName,
                        SecurityStamp = interviewer.SecurityStamp,
                        IsLockedBySupervisor = interviewer.IsLockedBySupervisor,
                        IsLockedByHeadquarters = interviewer.IsLockedByHeadquarters
                    };
                }
                else
                {
                    local.Email = interviewer.Email;
                    local.PasswordHash = interviewer.PasswordHash;
                    local.PhoneNumber = interviewer.PhoneNumber;
                    local.UserName = interviewer.UserName;
                    local.FullaName = interviewer.FullName;
                    local.SecurityStamp = interviewer.SecurityStamp;
                    local.IsLockedBySupervisor = interviewer.IsLockedBySupervisor;
                    local.IsLockedByHeadquarters = interviewer.IsLockedByHeadquarters;
                }

                this.interviewerViewRepository.Store(local);

                processedInterviewersCount++;

                Context.Progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Of_InterviewersFormat.FormatString(
                        processedInterviewersCount, remoteInterviewers.Count),
                    Statistics = Context.Statistics,
                    Status = SynchronizationStatus.Download
                });
            }

            Context.Progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_InterviewersFormat.FormatString(
                    processedInterviewersCount, remoteInterviewers.Count),
                Statistics = Context.Statistics,
                Status = SynchronizationStatus.Download
            });
        }
    }
}

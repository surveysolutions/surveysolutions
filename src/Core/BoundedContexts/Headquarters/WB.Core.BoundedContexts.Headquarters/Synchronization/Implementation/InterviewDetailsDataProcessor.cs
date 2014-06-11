using System;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Core.BoundedContexts.Headquarters.Synchronization.Implementation
{
    internal class InterviewDetailsDataProcessor : IInterviewDetailsDataProcessor
    {
        private readonly ILogger logger;
        private readonly InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext;
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;
        private readonly SyncSettings syncSettings;
        private readonly IReadSideRepositoryReader<InterviewData> interviewDetailsReader;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public InterviewDetailsDataProcessor(ILogger logger,
            InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext,
            InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings,
            SyncSettings syncSettings,
            IReadSideRepositoryReader<InterviewData> interviewDetailsReader,
            IFileSystemAccessor fileSystemAccessor)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (interviewDetailsDataProcessorContext == null)
                throw new ArgumentNullException("interviewDetailsDataProcessorContext");
            if (interviewDetailsDataLoaderSettings == null)
                throw new ArgumentNullException("interviewDetailsDataLoaderSettings");
            if (syncSettings == null) throw new ArgumentNullException("syncSettings");
            if (interviewDetailsReader == null) throw new ArgumentNullException("interviewDetailsReader");
            if (fileSystemAccessor == null) throw new ArgumentNullException("fileSystemAccessor");

            this.logger = logger;
            this.interviewDetailsDataProcessorContext = interviewDetailsDataProcessorContext;
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
            this.syncSettings = syncSettings;
            this.interviewDetailsReader = interviewDetailsReader;
            this.fileSystemAccessor = fileSystemAccessor;
        }


        public void Process()
        {
            var incomingCapiPackagesDirectory = this.fileSystemAccessor.CombinePath(syncSettings.AppDataDirectory,
                syncSettings.IncomingCapiPackagesDirectoryName);

            var allCapiPackages = this.fileSystemAccessor.GetFilesInDirectory(incomingCapiPackagesDirectory,
                string.Format("*.{0}", this.syncSettings.IncomingCapiPackageFileNameExtension));

            if (allCapiPackages.Length == 0)
            {
                interviewDetailsDataProcessorContext.PushMessage("Capi interview packages was not found");
                return;
            }

            interviewDetailsDataProcessorContext.PushMessage(string.Format("{0} capi interview package(s) was found",
                allCapiPackages.Length));

            var numberOfInterviewsForLoad =
                Math.Min(this.interviewDetailsDataLoaderSettings.NumberOfInterviewsProcessedAtTime,
                    allCapiPackages.Length);

            interviewDetailsDataProcessorContext.PushMessage(
                string.Format("{0} capi interview package(s) will be processed", numberOfInterviewsForLoad));

            int numberOfSuccessfullyLoadedInterviews = 0;
            for (int indexOfLoadedInterviews = 0; indexOfLoadedInterviews < allCapiPackages.Length; indexOfLoadedInterviews++)
            {
                if (numberOfSuccessfullyLoadedInterviews >
                    this.interviewDetailsDataLoaderSettings.NumberOfInterviewsProcessedAtTime)
                    break;

                var capiPackageFullPath = allCapiPackages[indexOfLoadedInterviews];

                var capiPackageFileName = this.fileSystemAccessor.GetFileName(capiPackageFullPath);
                var capiPackageFileNameWithoutExtension = this.fileSystemAccessor.GetFileNameWithoutExtension(capiPackageFullPath);
                var progressIndicatorForLoadedInterviews = string.Format("[{0} of {1}]", indexOfLoadedInterviews + 1,
                    numberOfInterviewsForLoad);

                interviewDetailsDataProcessorContext.PushMessage(string.Format("{0} load package {1}",
                    progressIndicatorForLoadedInterviews, capiPackageFileNameWithoutExtension));

                var numberOfInterviewsForLoadShifted = numberOfInterviewsForLoad < allCapiPackages.Length
                    ? numberOfInterviewsForLoad + 1
                    : allCapiPackages.Length;

                Guid interviewIdByPackageFileName;
                if (!Guid.TryParse(capiPackageFileNameWithoutExtension, out interviewIdByPackageFileName))
                {
                    numberOfInterviewsForLoad = numberOfInterviewsForLoadShifted;
                    interviewDetailsDataProcessorContext.PushMessage(string.Format("bad package name"));
                    continue;
                }

                try
                {
                    this.interviewDetailsReader.GetById(interviewIdByPackageFileName.ToString());
                }
                catch (Exception e)
                {
                    numberOfInterviewsForLoad = numberOfInterviewsForLoadShifted;

                    interviewDetailsDataProcessorContext.PushMessage(
                        string.Format("Unexpected error: {0}", e.Message));
                    this.logger.Error("Error when load interview from capi package", e);
                    continue;
                }

                var incomingCapiPackagesWithErrorsDirectoryFullPath =
                this.fileSystemAccessor.CombinePath(this.syncSettings.AppDataDirectory,
                    this.syncSettings.IncomingCapiPackagesWithErrorsDirectoryName);

                var capiPackageWithErrorsFullPath =
                    this.fileSystemAccessor.CombinePath(incomingCapiPackagesWithErrorsDirectoryFullPath, capiPackageFileName);

                if (this.fileSystemAccessor.IsFileExists(capiPackageWithErrorsFullPath))
                {
                    numberOfInterviewsForLoad = numberOfInterviewsForLoadShifted;
                    interviewDetailsDataProcessorContext.PushMessage(string.Format("bad package"));
                    continue;
                }

                interviewDetailsDataProcessorContext.PushMessage(string.Format("successfully loaded"));

                numberOfSuccessfullyLoadedInterviews++;
            }
        }
    }
}
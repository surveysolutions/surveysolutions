using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class MemoryStatusViewModel : MvxNotifyPropertyChanged
    {
        private readonly ILogger logger;
        private readonly IEnvironmentInformationUtils environmentInformationUtils;
        private string report;

        public MemoryStatusViewModel(ILogger logger, IEnvironmentInformationUtils environmentInformationUtils)
        {
            this.logger = logger;
            this.environmentInformationUtils = environmentInformationUtils;
        }

        public IMvxCommand CreateReportCommand => new MvxCommand(this.CreateReport);

        private void CreateReport()
        {
            var peers = environmentInformationUtils.GetPeersFormatted();
            var disk = environmentInformationUtils.GetDiskInformation();
            var ram = environmentInformationUtils.GetRAMInformation();
            var references = environmentInformationUtils.GetReferencesFormatted();
            
            Report = $"Disk:\n{disk}\n\nRAM:\n{ram}\n\nReferences:\n{references}\n\nPeers:\n{peers}"; 
        }

        public string Report
        {
            get => report;
            set => this.SetProperty(ref this.report, value);
        }
    }
}

using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WB.Tools.EventsMigrator
{
    public interface IShell
    {
        string ServerAddress { get; set; }
        string RavenDatabaseName { get; set; }
        int Processed { get; set; }
        int ProcessedPercent { get; set; }
        int? TotalEvents { get; set; }
        string ErrorMessage { get; set; }
        string Status { get; set; }
        string Elapsed { get; set; }
        string SelectedAppName { get; set; }
        bool CanTransfer { get; set; }
        ObservableCollection<string> ErrorMessages { get; set; }
        string EventStoreIP { get; set; }
        int EventStoreTcpPort { get; set; }
        int EventStoreHttpPort { get; set; }
        string EventStoreLogin { get; set; }
        string EventStorePassword { get; set; }
        ObservableCollection<string> AppNames { get; set; }
        int SkipEvents { get; set; }
        Task Transfer();
    }
}
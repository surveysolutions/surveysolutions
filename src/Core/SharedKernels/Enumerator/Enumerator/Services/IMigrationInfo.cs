namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IMigrationInfo
    {
        long Version { get; }
        string Description { get; }
        IMigration Migration { get; }
        string GetName();
    }
}

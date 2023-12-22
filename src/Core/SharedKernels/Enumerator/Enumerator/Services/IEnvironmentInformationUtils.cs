namespace WB.Core.SharedKernels.Enumerator.Services;

public interface IEnvironmentInformationUtils
{
    public string GetDiskInformation();
    public string GetRAMInformation();
    public string GetPeersFormatted();
    public string GetReferencesFormatted();
}

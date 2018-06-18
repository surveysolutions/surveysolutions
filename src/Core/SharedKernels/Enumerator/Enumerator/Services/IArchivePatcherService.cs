namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IArchivePatcherService
    {
        void ApplyPath(string oldFile, string patchIn, string newFileOut);
    }
}
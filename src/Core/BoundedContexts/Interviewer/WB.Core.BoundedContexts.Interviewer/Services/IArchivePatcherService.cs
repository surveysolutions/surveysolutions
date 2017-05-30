namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IArchivePatcherService
    {
        void ApplyPath(string oldFile, string pathIn, string newFileOut);
    }
}
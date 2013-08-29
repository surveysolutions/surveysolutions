namespace WB.Core.Infrastructure.Backup
{
    public interface IBackup
    {

        string Backup();
        void Restore();

        string RestorePath { get; }
    }
}
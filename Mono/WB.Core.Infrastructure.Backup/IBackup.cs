namespace WB.Core.Infrastructure.Backup
{
    public interface IBackup
    {

        void Backup();
        void Restore();

    }
}
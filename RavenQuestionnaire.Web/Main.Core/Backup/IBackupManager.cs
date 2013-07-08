namespace Main.Core.Backup
{
    /// <summary>
    /// The i backup manager.
    /// </summary>
    public interface IBackupManager
    {
        /// <summary>
        /// The do backup.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        string DoBackup();
    }
}
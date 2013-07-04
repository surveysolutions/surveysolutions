using Main.Core.Documents;

namespace Main.Core.Services
{
    /// <summary>
    /// The FileStorageService interface.
    /// </summary>
    public interface IFileStorageService
    {
        #region Public Methods and Operators

        /// <summary>
        /// The delete file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        void DeleteFile(string filename);

        /// <summary>
        /// The retrieve file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.FileDescription.
        /// </returns>
        FileDescription RetrieveFile(string filename);

        /// <summary>
        /// The retrieve thumb.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.FileDescription.
        /// </returns>
        FileDescription RetrieveThumb(string filename);

        /// <summary>
        /// The store file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        void StoreFile(FileDescription file);

        #endregion
    }
}
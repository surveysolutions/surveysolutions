namespace SynchronizationMessages.Synchronization
{
    using System.IO;

    /// <summary>
    /// The CustomSerializable interface.
    /// </summary>
    public interface ICustomSerializable
    {
        #region Public Methods and Operators

        /// <summary>
        /// The initialize from.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        void InitializeFrom(Stream stream);

        /// <summary>
        /// The write to.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        void WriteTo(Stream stream);

        #endregion
    }
}
using System.IO;

namespace ddidotnet
{
    /// <summary>
    ///     Interface for meta data descriptor that can save itself to a DDI XML file.
    /// </summary>
    public interface IMetaDescription
    {
        /// <summary>
        ///     Group of properties related to the whole DDI document
        /// </summary>
        DdiDocument Document { get; }

        /// <summary>
        ///     Group of properties related to the study
        /// </summary>
        DdiStudy Study { get; }

        /// <summary>
        ///     Add a new file to the DDI description
        /// </summary>
        /// <param name="name">Name of the file being added</param>
        /// <returns>Reference to the newly created data file descriptor.</returns>
        /// \b Example Adding files to DDI output
        /// \snippet Example2.cs Adding files to DDI output
        DdiDataFile AddDataFile(string name);

        /// <summary>
        ///     Writes DDI to a stream
        /// </summary>
        /// <param name="stream">Stream to be written to.</param>
        /// \b Example Writing to a stream
        /// \snippet Example2.cs Writing to a stream
        void WriteXml(Stream stream);

        /// <summary>
        ///     Writes DDI to a file
        /// </summary>
        /// <param name="filename">File name to be written to. If file exists it will be overwritten.</param>
        /// \b Example Writing to a file
        /// \snippet Example2.cs Writing to a file
        void WriteXml(string filename);
    }
}

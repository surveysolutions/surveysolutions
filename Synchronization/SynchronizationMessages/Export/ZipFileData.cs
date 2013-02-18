// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZipFileData.cs" company="">
//   
// </copyright>
// <summary>
//   The zip file data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SynchronizationMessages.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using Ionic.Zip;
    using Ionic.Zlib;

    using Main.Core.Events;

    using Newtonsoft.Json;

    /// <summary>
    /// The zip file data.
    /// </summary>
    public class ZipFileData // will be moved here later
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipFileData"/> class.
        /// </summary>
        public ZipFileData()
        {
            this.ImportDate = DateTime.Now;
            this.CreationDate = DateTime.UtcNow;
        }

        protected DateTime CreationDate
        {
            get;
            set;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the client guid.
        /// </summary>
        public Guid ClientGuid { get; set; }

        /// <summary>
        /// Gets or sets the events.
        /// </summary>
        public IEnumerable<AggregateRootEvent> Events { get; set; }

        /// <summary>
        /// Gets or sets the import date.
        /// </summary>
        public DateTime ImportDate { get; set; }

        #endregion

        /// <summary>
        /// The make valid file name.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The make valid file name.
        /// </returns>
        public static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
        }

        /// <summary>
        /// The events to string.
        /// </summary>
        /// <param name="clientGuid">
        /// The client guid.
        /// </param>
        /// <param name="events">
        /// The events.
        /// </param>
        /// <returns>
        /// The events to string
        /// </returns>
        public static string EventsToString(Guid? clientGuid, IEnumerable<AggregateRootEvent> events)
        {
            var data = new ZipFileData
                {
                    ClientGuid = clientGuid == null ? Guid.Empty : clientGuid.Value,
                    Events = events
                };
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            return JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        }

        /// <summary>
        /// The export internal.
        /// </summary>
        /// <param name="files">
        /// The files.
        /// </param>
        /// <param name="entryFileName">
        /// The entry file name.
        /// </param>
        /// <returns>
        /// Byte array
        /// </returns>
        public static byte[] ExportInternal(Dictionary<string, string> files, string entryFileName)
        {
            return ExportInternal(
                (zip) =>
                {
                    foreach (var file in files)
                    {
                        zip.AddEntry(MakeValidFileName(file.Key), file.Value);
                    }
                },
                entryFileName);
        }

        /// <summary>
        /// The export internal.
        /// </summary>
        /// <param name="files">
        /// The files.
        /// </param>
        /// <param name="entryFileName">
        /// The entry file name.
        /// </param>
        /// <returns>
        /// Byte array
        /// </returns>
        public static byte[] ExportInternal(Dictionary<string, byte[]> files, string entryFileName)
        {
            return ExportInternal(
                (zip) =>
                {
                    foreach (var file in files)
                    {
                        zip.AddEntry(MakeValidFileName(file.Key), file.Value);
                    }
                },
                entryFileName);
        }

        /// <summary>
        /// The export internal.
        /// </summary>
        /// <param name="data">
        /// The events.
        /// </param>
        /// <param name="fileName">
        /// The file Name.
        /// </param>
        /// <returns>
        /// Zip file as array of bytes
        /// </returns>
        public static byte[] ExportInternal(string data, string fileName)
        {
            return ExportInternal((zip) => zip.AddEntry(MakeValidFileName(fileName), data), fileName);
        }

        /// <summary>
        /// The export internal.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// Byte array
        /// </returns>
        public static byte[] ExportInternal(Stream data, string fileName)
        {
            return ExportInternal((zip) => zip.AddEntry(MakeValidFileName(fileName), data), fileName);
        }

        /// <summary>
        /// The export internal.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="entryFileName">
        /// The entry file name.
        /// </param>
        /// <returns>
        /// Byte array
        /// </returns>
        private static byte[] ExportInternal(Action<ZipFile> action, string entryFileName)
        {
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile(MakeValidFileName(entryFileName)))
            {
                // var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.CompressionLevel = CompressionLevel.BestCompression;
                action(zip);
                zip.Save(outputStream);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }
    }
}
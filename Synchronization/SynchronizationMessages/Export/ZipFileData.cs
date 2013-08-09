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

        public static string MakeValidFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(name, invalidReStr, "_");
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
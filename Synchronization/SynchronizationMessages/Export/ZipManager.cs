namespace SynchronizationMessages.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Ionic.Zip;

    /// <summary>
    /// The zip manager.
    /// </summary>
    public static class ZipManager
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get zip content.
        /// </summary>
        /// <param name="inputStream">
        /// The input stream.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GetZipContent(Stream inputStream)
        {
            if (!ZipFile.IsZipFile(inputStream, false))
            {
                return null;
            }

            inputStream.Position = 0;

            try
            {
                var list = new List<string>();

                using (ZipFile zip = ZipFile.Read(inputStream))
                {
                    using (var stream = new MemoryStream())
                    {
                        foreach (ZipEntry e in zip)
                        {
                            e.Extract(stream);
                        }
                        stream.Position = 0;
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string text = reader.ReadToEnd();
                            list.Add(text);
                        }
                    }
                }

                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using Ionic.Zip;
using System.IO;
using Ionic.Zlib;
using System.Web;
using System.Text;
using Newtonsoft.Json;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Events;


namespace Questionnaire.Core.Web.Export
{
    public interface IExportImport
    {
        void Import(HttpPostedFileBase uploadFile);
        byte[] Export(Guid clientGuid);
        /// <summary>
        /// return list of ALL events grouped by aggregate root, please use very carefully
        /// </summary>
        /// <returns></returns>
        byte[] ExportAllEvents(Guid clientGuid);
    }

    public class ExportImportEvent : IExportImport
    {

        #region FieldsProperties

        private IEventSync synchronizer;

        #endregion

        #region Constructor

        public ExportImportEvent(IEventSync synchronizer)
        {
            this.synchronizer = synchronizer;
        }

        #endregion

        #region PublicMethod

        public void Import(HttpPostedFileBase uploadFile)
        {
            if (ZipFile.IsZipFile(uploadFile.InputStream, false))
            {
                uploadFile.InputStream.Position = 0;

                ZipFile zip = ZipFile.Read(uploadFile.InputStream);
                using (var stream = new MemoryStream())
                {
                    foreach (ZipEntry e in zip)
                        e.Extract(stream);

                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                    var result = JsonConvert.DeserializeObject<ZipFileData>
                        (Encoding.Default.GetString(stream.ToArray()), settings);

                    synchronizer.WriteEvents(result.Events);
                }
            }
        }

        protected byte[] ExportInternal(Guid clientGuid, Func<IEnumerable<AggregateRootEventStream>> action)
        {
            var data = new ZipFileData
            {
                ClientGuid = clientGuid,
                Events = action()
            };

            var outputStream = new MemoryStream();

            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.CompressionLevel = CompressionLevel.None;
                string filename = string.Format("backup-{0}.txt", DateTime.Now.ToString().Replace(" ", "_"));

                zip.AddEntry(filename, JsonConvert.SerializeObject(data, Formatting.Indented, settings));
                zip.Save(outputStream);
            }

            outputStream.Seek(0, SeekOrigin.Begin);

            return outputStream.ToArray();
        }

        public byte[] Export(Guid clientGuid)
        {
            return ExportInternal(clientGuid, this.synchronizer.ReadCompleteQuestionare);
        }

        public byte[] ExportAllEvents(Guid clientGuid)
        {
            return ExportInternal(clientGuid, this.synchronizer.ReadEvents);
        }

        #endregion


    }
}
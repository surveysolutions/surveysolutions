using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Ionic.Zip;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Web.App_Start;
using RavenQuestionnaire.Core;
using Ncqrs;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace Questionnaire.Core.Web.Export
{
    public interface IExportImport
    {
        void Import(HttpPostedFileBase uploadFile);
        byte[] Export();
        byte[] Export(IViewRepository viewRepository);
    }
    
    public class ExportImportEvent:IExportImport
    {

        #region FieldsProperties

        private IClientSettingsProvider clientSettingsProvider;
        private IEventSync synchronizer;

        #endregion

        #region Constructor

        public ExportImportEvent(IClientSettingsProvider clientSettingsProvider, IEventSync synchronizer)
        {
            this.clientSettingsProvider = clientSettingsProvider;
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
                    var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
                    var result = JsonConvert.DeserializeObject<ZipFileData>
                        (Encoding.Default.GetString(stream.ToArray()), settings);
                    synchronizer.WriteEvents(result.Events);
                }
            }
        }
        public byte[] Export()
        {

            var data = new ZipFileData
            {
                ClientGuid = clientSettingsProvider.ClientSettings.PublicKey
            };
            data.Events = this.synchronizer.ReadEvents();



            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
                zip.AddEntry(string.Format("backup-{0}.txt", DateTime.Now.ToString().Replace(" ", "_")),
                                            JsonConvert.SerializeObject(data, Formatting.Indented, settings));
                zip.Save(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }
        public byte[] Export(IViewRepository viewRepository)
        {

            var data = new ZipFileData
                           {
                               ClientGuid = clientSettingsProvider.ClientSettings.PublicKey
                           };
            data.Events = this.synchronizer.ReadCompleteQuestionare(viewRepository);
            
            
            
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
                zip.AddEntry(string.Format("backup-{0}.txt", DateTime.Now.ToString().Replace(" ", "_")), 
                                            JsonConvert.SerializeObject(data,Formatting.Indented, settings));
                zip.Save(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }

        #endregion


    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Ionic.Zip;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using RavenQuestionnaire.Core.Views.Event;

namespace Questionnaire.Core.Web.Export
{
    public interface IExportImport
    {
        void Import(HttpPostedFileBase uploadFile);
        byte[] Export();
    }
    
    public class ExportImportEvent:IExportImport
    {

        #region FieldsProperties

        private IClientSettingsProvider clientSettingsProvider;

        #endregion

        #region Constructor

        public ExportImportEvent(IClientSettingsProvider clientSettingsProvider)
        {
            this.clientSettingsProvider = clientSettingsProvider;
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
                    var myEventBus = NcqrsEnvironment.Get<IEventBus>();
                    if (myEventBus == null)
                        throw new Exception("IEventBus is not properly initialized.");
                    foreach (CommittedEvent commitedEvent in result.Events)
                    {
                        try
                        {
                            myEventBus.Publish(commitedEvent);
                        }catch(Exception)
                        {
                        }
                    }
                    /*  var lastEventItem = viewRepository.Load<EventViewInputModel, EventView>(new EventViewInputModel(result.ClientGuid));
                    if (lastEventItem == null)
                        ExecuteCommand(result.Events, result.ClientGuid);
                    else
                    {
                        var ndx = result.Events.FindIndex(f => f.PublicKey == lastEventItem.PublicKey);
                        if (ndx > -1)
                        {
                            result.Events.RemoveRange(0, ndx+1);
                            ExecuteCommand(result.Events, result.ClientGuid);
                        }
                    }*/
                }
            }
        }

        public byte[] Export()
        {
            var myEventStore = NcqrsEnvironment.Get<IEventStore>();

            if (myEventStore == null)
                throw new Exception("IEventStore is not correct.");

            var data = new ZipFileData
                           {
                               ClientGuid = clientSettingsProvider.ClientSettings.PublicKey
                           };
            data.Events = myEventStore.ReadFrom(DateTime.MinValue);
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

        #region PrivateMethod

        private void ExecuteCommand(IEnumerable<CommittedEvent> items, Guid clientGuid)
        {
            /*foreach (CommittedEvent item in items)
                commandInvoker.Execute(item.Payload);
            commandInvoker.Flush();*/
        }

        #endregion

    }
}
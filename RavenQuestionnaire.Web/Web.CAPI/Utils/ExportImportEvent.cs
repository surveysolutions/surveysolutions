using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Newtonsoft.Json;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Event;
using RavenQuestionnaire.Core.ClientSettingsProvider;
using Web.CAPI.Models;

namespace Web.CAPI.Utils
{
    public interface IExportImport
    {
        void Import(HttpPostedFileBase uploadFile);
        byte[] Export();
    }
    
    public class ExportImportEvent:IExportImport
    {

        #region FieldsProperties

        private readonly IMemoryCommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;
        private IClientSettingsProvider clientSettingsProvider;

        #endregion

        #region Constructor

        public ExportImportEvent(IMemoryCommandInvoker memoryCommandInvoker, IViewRepository viewRepository, IClientSettingsProvider clientSettingsProvider)
        {
            this.commandInvoker = memoryCommandInvoker;
            this.viewRepository = viewRepository;
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
                    var lastEventItem = viewRepository.Load<EventViewInputModel, EventView>(new EventViewInputModel(result.ClientGuid));
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
                    }
                }
            }
        }

        public byte[] Export()
        {
            var data = new ZipFileData
                           {
                               ClientGuid = clientSettingsProvider.ClientSettings.PublicKey,
                               Events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(
                                                        new EventBrowseInputModel(null)).Items.OrderBy(x => x.CreationDate)
                                                        .ToList<EventBrowseItem>()
                           };
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

        private void ExecuteCommand(IEnumerable<EventBrowseItem> items, Guid clientGuid)
        {
            foreach (EventBrowseItem item in items)
                commandInvoker.Execute(item.Command, item.PublicKey, clientGuid);
            commandInvoker.Flush();
        }

        #endregion

    }
}
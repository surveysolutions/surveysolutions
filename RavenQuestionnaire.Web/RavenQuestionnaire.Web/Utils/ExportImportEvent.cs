using System;
using Ionic.Zip;
using System.IO;
using System.Web;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Event;

namespace RavenQuestionnaire.Web.Utils
{
    public interface IExportImport
    {
        void Import(HttpPostedFileBase uploadFile);
        byte[] Export();
    }


    public class ExportImportEvent:IExportImport
    {
        private readonly IMemoryCommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public ExportImportEvent(IMemoryCommandInvoker memoryCommandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = memoryCommandInvoker;
            this.viewRepository = viewRepository;
        }


        public void Import(HttpPostedFileBase uploadFile)
        {
            if (ZipFile.IsZipFile(uploadFile.InputStream, false))
            {
                    uploadFile.InputStream.Position = 0;
                    ZipFile zip = ZipFile.Read(uploadFile.InputStream);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        foreach (ZipEntry e in zip)
                            e.Extract(stream);
                        var settings = new JsonSerializerSettings();
                        settings.TypeNameHandling = TypeNameHandling.Objects;
                        var results = JsonConvert.DeserializeObject<List<EventBrowseItem>>
                            (Encoding.Default.GetString(stream.ToArray()), settings);
                        foreach (EventBrowseItem item in results)
                            commandInvoker.Execute(item.Command, item.PublicKey, Guid.NewGuid());
                        commandInvoker.Flush();
                    }
            }
        }

        public byte[] Export()
        {
            var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(null));
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Objects;
                zip.AddEntry(string.Format("{0}.txt", "events"), JsonConvert.SerializeObject(events.Items.OrderBy(x => x.CreationDate),
                                                                 Formatting.Indented, settings));
                zip.Save(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream.ToArray();
        }
    }
}
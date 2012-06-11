using System;
using System.ComponentModel;
using Ionic.Zip;
using System.IO;
using System.Web;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Event;

namespace RavenQuestionnaire.Web.Controllers
{
   public class ImportExportController : AsyncController
    {
        private readonly IMemoryCommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public ImportExportController(IMemoryCommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Import()
        {
            return View("ViewTestUploadFile");
        }

        /*************non async Import**********/
        //[AcceptVerbs(HttpVerbs.Post)]
        //public ActionResult Import(HttpPostedFileBase myfile)
        //{
        //    if (myfile != null && myfile.ContentLength != 0)
        //    {
        //        HttpPostedFileBase uploadedFile = Request.Files[0];
        //        if (ZipFile.IsZipFile(uploadedFile.InputStream, false))
        //        {
        //            uploadedFile.InputStream.Position = 0;
        //            ZipFile zip = ZipFile.Read(uploadedFile.InputStream);
        //            foreach (ZipEntry e in zip)
        //            {
        //                using (MemoryStream stream = new MemoryStream())
        //                {
        //                    e.Extract(stream);
        //                    var settings = new JsonSerializerSettings();
        //                    settings.TypeNameHandling = TypeNameHandling.Objects;
        //                    var results = JsonConvert.DeserializeObject<List<EventBrowseItem>>(Encoding.Default.GetString(stream.ToArray()), settings).OrderBy(x => x.CreationDate);
        //                    try
        //                    {
        //                        foreach (EventBrowseItem item in results)
        //                            commandInvoker.Execute(item.Command, item.PublicKey, Guid.NewGuid());
        //                        commandInvoker.Flush();
        //                    }
        //                    catch(Exception)
        //                    {
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return RedirectToAction("Index", "Dashboard");
        //}

        [AcceptVerbs(HttpVerbs.Post)]
        public void ImportAsync(HttpPostedFileBase myfile)
        {
            if (myfile != null && myfile.ContentLength != 0)
            {
                AsyncManager.OutstandingOperations.Increment();
                HttpPostedFileBase uploadedFile = Request.Files[0];
                if (ZipFile.IsZipFile(uploadedFile.InputStream, false))
                {
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += (o, e) =>
                                         {
                                             uploadedFile.InputStream.Position = 0;
                                             ZipFile zip = ZipFile.Read(uploadedFile.InputStream);
                                             AsyncManager.Parameters["files"] = zip;
                                         };
                    worker.RunWorkerAsync();
                }
                AsyncManager.OutstandingOperations.Decrement();
            }
        }


        public ActionResult ImportCompleted(ZipFile files)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (ZipEntry e in files)
                    e.Extract(stream);
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Objects;
                var results = JsonConvert.DeserializeObject<List<EventBrowseItem>>
                    (Encoding.Default.GetString(stream.ToArray()), settings);
                foreach (EventBrowseItem item in results)
                    commandInvoker.Execute(item.Command, item.PublicKey, Guid.NewGuid());
                commandInvoker.Flush();
            }
            return RedirectToAction("Index", "Dashboard");
        }

       

       /*************non async Export**********/
       //public FileResult Export()
       // {
       //     var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(null));
       //     var outputStream = new MemoryStream();
       //     using (var zip = new ZipFile())
       //     {
       //         using (MemoryStream stream = new MemoryStream())
       //         {
       //             var settings = new JsonSerializerSettings();
       //             settings.TypeNameHandling = TypeNameHandling.Objects;
       //             string mS = JsonConvert.SerializeObject(events.Items, Formatting.Indented, settings);
       //             //foreach (EventBrowseItem item in events.Items)
       //             //{
       //             //    var message = new EventSyncMessage
       //             //                      {
       //             //                          CommandKey = item.PublicKey,
       //             //                          Command = item.Command,
       //             //                          CreationDate = item.CreationDate
       //             //                      };
       //             //    message.WriteTo(stream);
       //             //}
       //             //zip.AddEntry(string.Format("{0}.txt", "events"), stream.ToArray());
       //             zip.AddEntry(string.Format("{0}.txt", "events"), mS);
       //         }
       //         zip.Save(outputStream);
       //     }
       //     outputStream.Seek(0, SeekOrigin.Begin);
       //     return File(outputStream, "application/zip", "events.zip");
       // }

        public void ExportAsync()
        {
            AsyncManager.OutstandingOperations.Increment();
            AsyncQuestionnaireUpdater.Update(() =>
            {
                try
                {
                    var events = viewRepository.Load<EventBrowseInputModel, EventBrowseView>(new EventBrowseInputModel(null));
                    AsyncManager.Parameters["result"] = events;
                }
                catch
                {
                    AsyncManager.Parameters["result"] = null;
                }
                AsyncManager.OutstandingOperations.Decrement();
            });
        }

        public ActionResult ExportCompleted(EventBrowseView result)
        {
            var outputStream = new MemoryStream();
            using (var zip = new ZipFile())
            {
                var settings = new JsonSerializerSettings();
                settings.TypeNameHandling = TypeNameHandling.Objects;
                zip.AddEntry(string.Format("{0}.txt", "events"), JsonConvert.SerializeObject(result.Items.OrderBy(x => x.CreationDate), Formatting.Indented, settings));
                zip.Save(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            return File(outputStream, "application/zip", "events.zip");
        }
    }
}

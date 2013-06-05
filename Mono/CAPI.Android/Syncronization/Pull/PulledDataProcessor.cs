using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ChangeLog;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using SynchronizationMessages.Synchronization;

namespace CAPI.Android.Syncronization.Pull
{
    public class PulledDataProcessor
    {
        public PulledDataProcessor(IChangeLogManipulator changelog, ICommandService commandService)
        {
            this.changelog = changelog;
            this.commandService = commandService;
        }

        private readonly IChangeLogManipulator changelog;
        private readonly ICommandService commandService;
        public void Save(string content, Guid chunckId)
        {
            var path = GetFileName(chunckId);
            using (var fs = File.Open(path, FileMode.CreateNew))
            {
                var bytes = GetBytes(content);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public void Proccess(Guid chunckId)
        {
            var path = GetFileName(chunckId);
            var unzippedData = string.Empty;
            using (var fs = File.Open(path, FileMode.Open))
            {
                unzippedData = PackageHelper.Decompress(fs) ;
            }

            if(string.IsNullOrEmpty(unzippedData))
                return;
            /*var command = new UploadSupervisorData(unzippedData);
            commandService.Execute(command);
            changelog.CreatePublicRecord(chunckId, command.EventSourceId);*/

            throw new NotImplementedException("implement excecution logic");
            File.Delete(path);
        }

        private string GetFileName(Guid id)
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),"sync_temp",
                                          id.ToString());
        }
        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
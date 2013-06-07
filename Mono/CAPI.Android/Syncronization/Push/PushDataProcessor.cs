using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ChangeLog;
using SynchronizationMessages.Synchronization;

namespace CAPI.Android.Syncronization.Push
{
    public class PushDataProcessor
    {
        private readonly IChangeLogManipulator changelog;

        public PushDataProcessor(IChangeLogManipulator changelog)
        {
            this.changelog = changelog;
        }

        private const int chunkCount = 10;
        
        public IList<ChunckDescription> GetChuncks()
        {
            var retval = new List<ChunckDescription>();
            for (int i = 0; i < chunkCount; i++)
            {
                var chunckId = Guid.NewGuid();
                
                retval.Add(new ChunckDescription(chunckId, PackageHelper.Compress(string.Format("hello capi push {0}", chunckId))));
            }
            return retval;
        }

        public void MarkChunckAsPushed(Guid chunckId)
        {
            changelog.MarkDraftChangesetAsPublic(chunckId);
        }
    }

}
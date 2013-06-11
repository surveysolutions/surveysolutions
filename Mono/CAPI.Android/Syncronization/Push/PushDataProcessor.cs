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

        
        public IList<ChunckDescription> GetChuncks()
        {
            var chunksIds = changelog.GetClosedDraftChunksIds();
            var retval = new List<ChunckDescription>();
            for (int i = 0; i < chunksIds.Count; i++)
            {

                retval.Add(new ChunckDescription(chunksIds[i], changelog.GetDraftRecordContent(chunksIds[i])));
            }
            return retval;
        }

        public void MarkChunckAsPushed(Guid chunckId)
        {
            changelog.MarkDraftChangesetAsPublic(chunckId);
        }
    }

}
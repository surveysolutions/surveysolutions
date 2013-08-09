using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model;
using CAPI.Android.Services;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Syncronization.Push
{
    public class PushDataProcessor
    {
        private readonly IChangeLogManipulator changelog;
        public PushDataProcessor(IChangeLogManipulator changelog)
        {
            this.changelog = changelog;
        }

        public IList<ChangeLogRecordWithContent> GetChuncks()
        {
            var records =  changelog.GetClosedDraftChunksIds();
            return records.Select(chunk => new ChangeLogRecordWithContent(chunk.RecordId, chunk.EventSourceId, changelog.GetDraftRecordContent(chunk.RecordId))).ToList();
        }

        public void DeleteInterview(Guid itemId)
        {
            new CleanUpExecutor(CapiApplication.Kernel.Get<IChangeLogManipulator>()).DeleteInterveiw(itemId);
        }
    }

    public class ChangeLogRecordWithContent:ChangeLogShortRecord
    {
        public ChangeLogRecordWithContent(Guid recordId, Guid eventSourceId, string content) : base(recordId, eventSourceId)
        {
            Content = content;
        }

        public string Content { get; private set; }
    }
}
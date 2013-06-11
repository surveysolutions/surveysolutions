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
using Main.Core.Commands.Questionnaire.Completed;
using Ncqrs.Commanding.ServiceModel;
using SynchronizationMessages.Synchronization;

namespace CAPI.Android.Syncronization.Push
{
    public class PushDataProcessor
    {
        private readonly IChangeLogManipulator changelog;
        private readonly ICommandService commandService;
        public PushDataProcessor(IChangeLogManipulator changelog, ICommandService commandService)
        {
            this.changelog = changelog;
            this.commandService = commandService;
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
            var arId = changelog.MarkDraftChangesetAsPublicAndReturnARId(chunckId);
            commandService.Execute(new DeleteCompleteQuestionnaireCommand(arId));
        }
    }

}
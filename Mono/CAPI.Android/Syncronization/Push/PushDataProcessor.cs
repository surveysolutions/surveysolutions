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
using WB.Core.Synchronization;

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


        public IList<SyncPackage> GetChuncks()
        {
            var chunks = changelog.GetClosedDraftChunksIds();
            var retval = new List<SyncPackage>();
            foreach (var chunk in chunks)
            {
                retval.Add(new SyncPackage()
                    {
                        Id = chunk.Key,
                        Status = true,
                        ItemsContainer =
                            new List<SyncItem>()
                                {
                                    new SyncItem()
                                        {
                                            Content = changelog.GetDraftRecordContent(chunk.Key),
                                            IsCompressed = true,
                                            ItemType = SyncItemType.Questionnare,
                                            Id = chunk.Value
                                        }
                                }
                    });
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
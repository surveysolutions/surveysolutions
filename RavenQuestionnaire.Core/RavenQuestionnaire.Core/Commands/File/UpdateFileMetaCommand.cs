using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.File
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (FileAR), "UpdateFileMeta")]
    public class UpdateFileMetaCommand : CommandBase
    {
        public UpdateFileMetaCommand(Guid publicKey, string title, string desc)
        {
            PublicKey = publicKey;
            Description = desc;
            Title = title;
        }
        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public string Title { get; private set; }

        public string Description { get; private set; }
    }
}
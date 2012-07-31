using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.File
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(FileAR), "DeleteFile")]
    public class DeleteFileCommand : CommandBase
    {
        public DeleteFileCommand(Guid publicKey)
        {
            PublicKey = publicKey;
        }
        [AggregateRootId]
        public Guid PublicKey { get; private set; }

    }
}
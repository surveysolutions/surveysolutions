using System;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface ICompleteGroup : IGroup
    {
        Guid? PropogationPublicKey { get; set; }
        bool Enabled { get; set; }
    }
}

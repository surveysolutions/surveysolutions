using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface IPropogate : ICloneable
    {
        Guid PropogationPublicKey { get; set; }
    /*    void Propogate(Guid childGroupPublicKey);
        void RemovePropogated(Guid childGroupPublicKey);*/
    }
}

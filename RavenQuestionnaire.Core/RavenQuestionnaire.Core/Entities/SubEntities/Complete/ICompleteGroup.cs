using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface ICompleteGroup : IGroup
    {
        ICompleteGroup ParentGroup { get; set; }
        ICompleteGroup NextGroup { get; set; }
        ICompleteGroup PrevGroup { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface ICompleteGroup : IGroup
    {
    }

    public interface ICompleteGroup<TGroup, TQuestion> : ICompleteGroup, IGroup<TGroup, TQuestion>
        where TQuestion : ICompleteQuestion
        where TGroup : ICompleteGroup
    {
    }
}

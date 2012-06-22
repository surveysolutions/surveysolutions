using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views
{
    public interface ICompositeView
    {
        Guid PublicKey { get; set; }

        string Title { get; set; }

        Guid? Parent { get; set; }

        List<ICompositeView> Children { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Services
{
    public interface IValildationService
    {
         bool Validate(CompleteQuestionnaire entity, Guid? groupKey, Guid? propagationKey);
    }
}

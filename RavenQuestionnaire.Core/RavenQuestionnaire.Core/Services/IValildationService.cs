using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Services
{
    public interface IValildationService
    {
         void Validate(CompleteQuestionnaire entity, Guid? groupKey, Guid? propagationKey);
    }
}

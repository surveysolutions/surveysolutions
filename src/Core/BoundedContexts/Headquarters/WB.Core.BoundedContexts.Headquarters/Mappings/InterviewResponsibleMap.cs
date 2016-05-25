﻿using System;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [Obsolete]
    public class InterviewResponsibleMap : ClassMapping<InterviewResponsible>
    {
        public InterviewResponsibleMap()
        {
            Table("InterviewResponsibles");
            Id(x => x.Id, mapper => mapper.Generator(Generators.Assigned));
            Property(x => x.InterviewId);
            Property(x => x.UserId);
        }
    }
}
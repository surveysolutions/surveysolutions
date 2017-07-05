﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class ReadonlyUserMap : ClassMapping<ReadonlyUser>
    {
        public ReadonlyUserMap()
        {
            Schema("users");
            Table("users");
            Id(x => x.Id, map =>
            {
                map.Generator(Generators.Assigned);
                map.Column("\"Id\"");
            });
            Property(x => x.Name, map => map.Column("\"UserName\""));
            this.ManyToOne(x => x.ReadonlyProfile, oto =>
            {
                oto.Cascade(Cascade.None);
                oto.Column("\"UserProfileId\"");
            });
            this.Set(x => x.RoleIds, map =>
            {
                map.Table("userroles");
                map.Schema("users");
                map.Cascade(Cascade.None);
                map.Key(k => k.Column("\"UserId\""));
            }, m =>
            {
                m.Element(el => el.Column("\"RoleId\""));
            });
        }
    }

    [PlainStorage]
    public class ProfileMap : ClassMapping<ReadonlyProfile>
    {
        public ProfileMap()
        {
            Id(x => x.Id, m => m.Column("\"Id\""));
            Table("userprofiles");
            Schema("users");

            Property(x => x.SupervisorId, p => p.Column("\"SupervisorId\""));
        }
    }
}
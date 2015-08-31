using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Synchronization.Documents;

namespace WB.Core.Synchronization.Mapping
{
    [PlainStorage]
    public class TabletSyncLogByUsersMap : ClassMapping<TabletSyncLog>
    {
        public TabletSyncLogByUsersMap()
        {
            Id(x => x.DeviceId);
            Property(x => x.AndroidId);
            Property(x => x.LastUpdateDate);
            Property(x => x.RegistrationDate);

            Set(x => x.RegisteredUsersOnDevice, m =>
            {
                m.Key(km => km.Column("DeviceId"));
                m.Lazy(CollectionLazy.NoLazy);
            },
                r => r.Element(e =>
                {
                    e.Column("UserId");
                }));

            List(x => x.UserSyncLog, set =>
            {
                set.Index(index => index.Column("Position"));
                set.Key(key => key.Column("DeviceId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                relation => relation.OneToMany());
        }
    }
    [PlainStorage]
    public class TabletSyncLogByUserMap : ClassMapping<TabletSyncLogByUser>
    {
        public TabletSyncLogByUserMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.HandshakeTime);
            Property(x => x.AppVersion);
            Property(x => x.UserId);
            ManyToOne(x => x.TabletSyncLog, mto =>
            {
                mto.Index("TabletSyncLogs_TabletSyncLogByUsers");
                mto.Cascade(Cascade.None);
            });

            List(x => x.PackagesTrackingInfo, set =>
            {
                set.Index(index => index.Column("Position"));
                set.Key(key => key.Column("TabletSyncLogId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
               relation => relation.OneToMany());
        }
    }
    [PlainStorage]
    public class SyncPackageTrackingInfoMap : ClassMapping<SyncPackageTrackingInfo>
    {
        public SyncPackageTrackingInfoMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.PackageId);
            Property(x => x.PackageSyncTime);
            Property(x => x.PackageType);
            ManyToOne(x => x.TabletSyncLogByUser, mto =>
            {
                mto.Index("TabletSyncLogByUsers_SyncPackageTrackingInfos");
                mto.Cascade(Cascade.None);
            });
        }
    }
}
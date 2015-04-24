using Main.Core.Entities.SubEntities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Mappings
{
    public class UserDocumentMap : ClassMapping<UserDocument>
    {
        public UserDocumentMap()
        {
            Table("UserDocuments");
            Id(x => x.UserId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            Property(x => x.PublicKey);
            Property(x => x.CreationDate);
            Property(x => x.Email);
            Property(x => x.IsDeleted);
            Property(x => x.IsLockedByHQ);
            Property(x => x.IsLockedBySupervisor);
            Property(x => x.Password);
            Property(x => x.UserName);
            Property(x => x.LastChangeDate);
            Property(x => x.DeviceId);
            Component(x => x.Supervisor, cmp =>
            {
                cmp.Property(x => x.Id, ptp => ptp.Column("SupervisorId"));
                cmp.Property(x => x.Name, ptp => ptp.Column("SupervisorName"));
            });

            Set(x => x.Roles, m =>
            {
                m.Key(km => km.Column("UserId"));
                m.Table("Roles");
                m.Lazy(CollectionLazy.NoLazy);
            },
            r => r.Element(e =>
            {
                e.Column("RoleId");
            }));

            Set(x => x.DeviceChangingHistory, set => {
                set.Key(key => key.Column("UserId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
            relation => relation.OneToMany());
        }
    }

    public class DeviceInfoMap : ClassMapping<DeviceInfo>
    {
        public DeviceInfoMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.Date);
            Property(x => x.DeviceId);
            ManyToOne(x => x.User, mto =>
            {
                mto.Index("UserDocuments_DeviceInfos");
                mto.Cascade(Cascade.None);
            });
        }
    }
}
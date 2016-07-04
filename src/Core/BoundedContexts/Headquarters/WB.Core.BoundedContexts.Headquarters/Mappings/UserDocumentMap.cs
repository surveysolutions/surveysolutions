using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
    public class UserDocumentMap : ClassMapping<UserDocument>
    {
        public UserDocumentMap()
        {
            this.Table("UserDocuments");
            this.Id(x => x.UserId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });
            this.Property(x => x.PublicKey, cm =>
            {
                cm.Index("User_PublicKey");
                cm.Unique(true);
            });
            this.Property(x => x.CreationDate, cm => cm.Index("User_CreationDate"));
            this.Property(x => x.Email, cm => cm.Index("User_Email"));
            this.Property(x => x.IsLockedByHQ);
            this.Property(x => x.IsLockedBySupervisor);
            this.Property(x => x.IsArchived);
            this.Property(x => x.Password, cm => cm.Index("User_UserName_Password"));
            this.Property(x => x.UserName, cm => cm.Index("User_UserName_Password"));
            this.Property(x => x.LastChangeDate);
            this.Property(x => x.DeviceId, cm => cm.Index("User_DeviceId"));
            this.Property(x => x.PersonName, cm => cm.Index("User_PersonName"));
            this.Property(x => x.PhoneNumber);
            this.Component(x => x.Supervisor, cmp =>
            {
                cmp.Property(x => x.Id, ptp => { ptp.Column("SupervisorId"); ptp.Index("User_SupervisorId"); });
              
                cmp.Property(x => x.Name, ptp => ptp.Column("SupervisorName"));
            });

            this.Set(x => x.Roles, m =>
            {
                m.Key(km => km.Column(cm =>
                {
                    cm.Name("UserId");
                    cm.Index("Users_Roles_fk");
                }));
                m.Table("Roles");
                m.Lazy(CollectionLazy.NoLazy);
            },
            r => r.Element(e =>
            {
                e.Column("RoleId");
            }));

            this.Set(x => x.DeviceChangingHistory, set => {
                set.Key(key => key.Column("UserId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
            relation => relation.OneToMany());
        }
    }
    [PlainStorage]
    public class DeviceInfoMap : ClassMapping<DeviceInfo>
    {
        public DeviceInfoMap()
        {
            this.Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            this.Property(x => x.Date);
            this.Property(x => x.DeviceId);
            this.ManyToOne(x => x.User, mto =>
            {
                mto.Index("UserDocuments_DeviceInfos");
                mto.Cascade(Cascade.None);
            });
        }
    }
}
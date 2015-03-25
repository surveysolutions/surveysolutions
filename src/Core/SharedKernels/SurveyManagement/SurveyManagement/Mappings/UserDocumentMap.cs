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
             r => r.Element(e => e.Column("RoleId")));

            Set(x => x.DeviceChangingHistory, // TODO: This can be soted in hstore
             bagMap =>
             {
                 bagMap.Table("DeviceInfos");
                 bagMap.Key(key => key.Column("UserId"));
             },
             relation => relation.Component(element =>
             {
                 element.Property(x => x.Date);
                 element.Property(x => x.DeviceId);
             }));
        }
    }
}
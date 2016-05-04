using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Mapping
{
    [PlainStorage]
    public class UserPreloadingProcessMap : ClassMapping<UserPreloadingProcess>
    {
        public UserPreloadingProcessMap()
        {
            Id(x => x.UserPreloadingProcessId, idMap =>
            {
                idMap.Generator(Generators.Assigned);
                idMap.Column("Id");
            });

            Property(x => x.FileName);
            Property(x => x.State);
            Property(x => x.FileSize);
            Property(x => x.UploadDate);
            Property(x => x.LastUpdateDate);
            Property(x => x.RecordsCount);
            Property(x => x.CreatedUsersCount);
            Property(x => x.ErrorMessage);

            Property(x => x.ValidationStartDate);
            Property(x => x.VerificationProgressInPercents);
            Property(x => x.CreationStartDate);

            List<UserPreloadingDataRecord>(x => x.UserPrelodingData, listMap =>
            {
                listMap.Index(index => index.Column("Position"));
                listMap.Key(keyMap =>
                {
                    keyMap.Column(clm =>
                    {
                        clm.Name("UserPreloadingProcessId");
                        clm.Index("UserPreloadingDataRecords_UserPreloadingProcesses");
                    });
                });
                listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                rel =>
                {
                    rel.Component(cmp =>
                    {
                        cmp.Property(x => x.Login);
                        cmp.Property(x => x.Email);
                        cmp.Property(x => x.FullName);
                        cmp.Property(x => x.Password);
                        cmp.Property(x => x.PhoneNumber);
                        cmp.Property(x => x.Role);
                        cmp.Property(x => x.Supervisor);
                    });
                });

            Set(x => x.VerificationErrors, set =>
            {
                set.Key(key => key.Column("UserPreloadingProcessId"));
                set.Lazy(CollectionLazy.NoLazy);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
                relation => relation.OneToMany());
        }
    }
}
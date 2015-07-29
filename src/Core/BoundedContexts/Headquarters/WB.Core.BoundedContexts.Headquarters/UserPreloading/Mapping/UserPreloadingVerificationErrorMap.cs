using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Mapping
{
    [PlainStorage]
    public class UserPreloadingVerificationErrorMap : ClassMapping<UserPreloadingVerificationError>
    {
        public UserPreloadingVerificationErrorMap()
        {
            Id(x => x.Id, Idmap => Idmap.Generator(Generators.HighLow));
            Property(x => x.CellValue);
            Property(x => x.Code);
            Property(x => x.ColumnName);
            Property(x => x.RowNumber);
            ManyToOne(x => x.UserPreloadingProcess, mto =>
            {
                mto.Index("UserPreloadingVerificationErrors_UserPreloadingProcesses");
                mto.Cascade(Cascade.None);
            });
        }
    }
}
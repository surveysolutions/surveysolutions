using System.Data;
using NpgsqlTypes;

namespace WB.Infrastructure.Native.Storage.Postgre.NhExtensions
{
    public class Int64ArrayType : ArrayType<long>
    {
        protected override NpgSqlType GetNpgSqlType() => new NpgSqlType(
            DbType.Object,
            NpgsqlDbType.Array | NpgsqlDbType.Bigint
        );

    }
}
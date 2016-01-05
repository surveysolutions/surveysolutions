using System.Data;
using NHibernate.SqlTypes;
using NpgsqlTypes;

namespace WB.Core.Infrastructure.Storage.Postgre.NhExtensions
{
    public class NpgsqlExtendedSqlType : SqlType
    {
        private readonly NpgsqlDbType _npgDbType;

        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType) : base(dbType)
        {
            this._npgDbType = npgDbType;
        }

        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, int length) : base(dbType, length)
        {
            this._npgDbType = npgDbType;
        }

        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, byte precision, byte scale) : base(dbType, precision, scale)
        {
            this._npgDbType = npgDbType;
        }

        public NpgsqlDbType NpgDbType => this._npgDbType;
    }
}
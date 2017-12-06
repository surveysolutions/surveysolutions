using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Driver;
using NHibernate.SqlTypes;
using Npgsql;

namespace WB.Infrastructure.Native.Storage.Postgre.NhExtensions
{
    public class NpgsqlDriverExtended : NpgsqlDriver
    {
        protected override void InitializeParameter(DbParameter dbParam, string name, SqlType sqlType)
        {
            if (sqlType is NpgsqlExtendedSqlType && dbParam is NpgsqlParameter)
            {
                this.InitializeParameter((NpgsqlParameter) dbParam, name, (NpgsqlExtendedSqlType) sqlType);
            }
            else
            {
                base.InitializeParameter(dbParam, name, sqlType);
            }
        }

        protected virtual void InitializeParameter(NpgsqlParameter dbParam, string name, NpgsqlExtendedSqlType sqlType)
        {
            if (sqlType == null)
            {
                throw new QueryException($"No type assigned to parameter '{name}'");
            }

            dbParam.ParameterName = this.FormatNameForParameter(name);
            dbParam.DbType = sqlType.DbType;
            dbParam.NpgsqlDbType = sqlType.NpgDbType;
        }
    }
}
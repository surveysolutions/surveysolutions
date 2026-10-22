using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Type;

namespace WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

[Serializable]
public class PostgresDateType : DateType
{
    public override string Name => "PostgresDate";

    protected override DateTime GetDateTime(DbDataReader rs, int index, ISessionImplementor session)
    {
        // Npgsql 10 returns DateOnly for PostgreSQL date; older versions return DateTime.
        return rs[index] is DateOnly date
            ? date.ToDateTime(TimeOnly.MinValue)
            : base.GetDateTime(rs, index, session);
    }
}

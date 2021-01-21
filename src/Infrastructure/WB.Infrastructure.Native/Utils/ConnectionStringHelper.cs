using System;
using Npgsql;

namespace WB.Infrastructure.Native.Utils
{
    public static class ConnectionStringHelper
    {
        public static NpgsqlConnectionStringBuilder SetApplicationPostfix(this NpgsqlConnectionStringBuilder builder,
            string postfix)
        {
            builder.ApplicationName = (builder.ApplicationName ?? String.Empty) + "_" + postfix;
            return builder;
        }
    }
}

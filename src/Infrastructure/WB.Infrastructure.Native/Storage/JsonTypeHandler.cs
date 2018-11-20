using System;
using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage
{
    public class JsonHandler<T> : SqlMapper.TypeHandler<T> where T: class
    {
        public override T Parse(object value) => value == null || value == DBNull.Value
            ? default(T)
            : new EntitySerializer<T>().Deserialize(value as string);

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            if (value == null)
                parameter.Value = DBNull.Value;
            else
            {
                parameter.Value = new EntitySerializer<T>().Serialize(value);
                ((NpgsqlParameter)parameter).NpgsqlDbType = NpgsqlDbType.Jsonb;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NpgsqlTypes;

namespace WB.Core.Infrastructure.Storage.Postgre.NhExtensions
{
    public class PostgreSQL91Dialect : PostgreSQL82Dialect
    {
        public PostgreSQL91Dialect()
        {
            RegisterColumnType(DbType.Object, "text[]");
            RegisterColumnType(DbType.Object, "numeric[]");
        }
    }

    public class PostgresSqlArrayType<T> : IUserType
    {
        bool IUserType.Equals(object x, object y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x?.GetHashCode() ?? 0;
        }

        public virtual object NullSafeGet(IDataReader resultSet, string[] names, object owner)
        {
            var index = resultSet.GetOrdinal(names[0]);

            if (resultSet.IsDBNull(index))
            {
                return null;
            }

            T[] res = resultSet.GetValue(index) as T[];

            if (res != null)
            {
                return res;
            }

            throw new NotImplementedException();
        }

        public virtual void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            var parameter = ((IDbDataParameter)cmd.Parameters[index]);
            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                var list = (T[])value;
                parameter.Value = list;
            }
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public SqlType[] SqlTypes
        {
            get
            {
                var sqlTypes = new SqlType[]
                {
                    new NpgsqlExtendedSqlType(
                        DbType.Object,
                        NpgSqlType
                        )
                };

                return sqlTypes;
            }
        }

        private static readonly Dictionary<Type, NpgsqlTypes.NpgsqlDbType> TypesMatcher = new Dictionary<Type, NpgsqlDbType>()
        {
            { typeof(string[]), NpgsqlDbType.Array | NpgsqlDbType.Text },
            { typeof(decimal[]), NpgsqlDbType.Array | NpgsqlDbType.Numeric }
        }; 
        protected virtual NpgsqlTypes.NpgsqlDbType NpgSqlType
        {
            get
            {
                var targetTypeName = this.ReturnedType;
                if (!TypesMatcher.ContainsKey(targetTypeName))
                {
                    throw new KeyNotFoundException($"Add mapping of .net type {targetTypeName} to postgresql type.");
                }

                return TypesMatcher[targetTypeName];
            }
        }

        public virtual Type ReturnedType => typeof(T[]);

        public bool IsMutable { get; private set; }
    }
}
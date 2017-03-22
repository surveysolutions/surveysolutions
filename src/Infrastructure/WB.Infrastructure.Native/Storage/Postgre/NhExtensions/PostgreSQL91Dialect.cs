using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NpgsqlTypes;

namespace WB.Infrastructure.Native.Storage.Postgre.NhExtensions
{
    public class PostgreSQL91Dialect : PostgreSQL82Dialect
    {
        public PostgreSQL91Dialect()
        {
            this.RegisterColumnType(DbType.Object, "text[]");
            this.RegisterColumnType(DbType.Object, "numeric[]");
            this.RegisterColumnType(DbType.Object, "integer[]");
        }
    }

    public class IpAddressAsString : IUserType
    {
        bool IUserType.Equals(object x, object y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x?.GetHashCode() ?? 0;
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            object obj = NHibernateUtil.String.NullSafeGet(rs, names);
            if (obj == null)
            {
                return null;
            }
            return IPAddress.Parse(obj.ToString());
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value == null)
            {
                ((IDataParameter)cmd.Parameters[index]).Value = DBNull.Value;
            }
            else
            {
                ((IDataParameter)cmd.Parameters[index]).Value = value.ToString();
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

        public SqlType[] SqlTypes => new SqlType[] { SqlTypeFactory.GetString(15) };

        public Type ReturnedType => typeof(IPAddress);

        public bool IsMutable { get; private set; }
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
                        this.NpgSqlType
                        )
                };

                return sqlTypes;
            }
        }

        private static readonly Dictionary<Type, NpgsqlTypes.NpgsqlDbType> TypesMatcher = new Dictionary<Type, NpgsqlDbType>()
        {
            { typeof(string[]), NpgsqlDbType.Array | NpgsqlDbType.Text },
            { typeof(decimal[]), NpgsqlDbType.Array | NpgsqlDbType.Numeric },
            { typeof(int[]), NpgsqlDbType.Array | NpgsqlDbType.Integer }
        }; 
        protected virtual NpgsqlTypes.NpgsqlDbType NpgSqlType
        {
            get
            {
                var targetTypeName = this.ReturnedType;
                if (!TypesMatcher.ContainsKey(targetTypeName))
                {
                    throw new KeyNotFoundException($"Add mapping of .net type {targetTypeName} to postgresql type in {typeof(PostgreSQL91Dialect)}");
                }

                return TypesMatcher[targetTypeName];
            }
        }

        public virtual Type ReturnedType => typeof(T[]);

        public bool IsMutable { get; private set; }
    }
}
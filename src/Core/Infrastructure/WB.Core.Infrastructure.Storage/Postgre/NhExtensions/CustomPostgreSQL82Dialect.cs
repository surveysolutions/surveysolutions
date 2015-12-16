using System;
using System.Data;
using NHibernate.Dialect;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace WB.Core.Infrastructure.Storage.Postgre.NhExtensions
{
    public class CustomPostgreSQL82Dialect : PostgreSQL82Dialect
    {
        public CustomPostgreSQL82Dialect()
        {
            RegisterColumnType(DbType.Object, "text[]");
            RegisterColumnType(DbType.Object, "text[][]");
        }
    }


    public class PostgresSqlStringArrayType : IUserType
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

            string[] res = resultSet.GetValue(index) as string[];

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
                var list = (string[])value;
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
                        NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text
                        )
                };

                return sqlTypes;
            }
        }

        public virtual Type ReturnedType => typeof(string[]);

        public bool IsMutable { get; private set; }
    }

    public class PostgresSqlStringArrayArrayType : IUserType
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

            string[][] res = resultSet.GetValue(index) as string[][];

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
                var list = (string[][])value;
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
                        NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text
                        )
                };

                return sqlTypes;
            }
        }

        public virtual Type ReturnedType => typeof(string[][]);

        public bool IsMutable { get; private set; }
    }
}
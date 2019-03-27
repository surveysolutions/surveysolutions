using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Infrastructure.Native.Storage.Postgre.NhExtensions
{
    public abstract class ArrayType<T> : IUserType
    {
        public SqlType[] SqlTypes => new SqlType[] { GetNpgSqlType() };

        public Type ReturnedType => typeof(T[]);

        public bool IsMutable => true;

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object DeepCopy(object value)
        {
            var arr = value as T[];
            if (arr == null)
            {
                return null;
            }
            var result = new T[arr.Length];
            Array.Copy(arr, result, arr.Length);
            return result;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public new bool Equals(object x, object y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }

            var a = (T[]) x;
            var b = (T[]) y;

            return a.SequenceEqual(b);
        }

        public int GetHashCode(object x)
        {
            if (x == null)
            {
                return 0;
            }
            return x.GetHashCode();
        }

        public object NullSafeGet(
            DbDataReader rs,
            string[] names,
            ISessionImplementor session,
            object owner)
        {
            if (names.Length != 1)
            {
                throw new InvalidOperationException("Only expecting one column...");
            }
            return rs[names[0]] as T[];
        }

        public void NullSafeSet(
            DbCommand cmd,
            object value,
            int index,
            ISessionImplementor session
        )
        {
            var parameter = ((NpgsqlParameter)cmd.Parameters[index]);
            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.NpgsqlDbType = GetNpgSqlType().NpgDbType;

                switch (value)
                {
                    case T[] arr:
                        parameter.Value = arr;
                        break;
                    default:
                        throw new InvalidOperationException(
                            $"\"{parameter.ParameterName}\" is not {typeof(T)}[]"
                        );
                }
            }
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        protected abstract NpgSqlType GetNpgSqlType();
    }
}

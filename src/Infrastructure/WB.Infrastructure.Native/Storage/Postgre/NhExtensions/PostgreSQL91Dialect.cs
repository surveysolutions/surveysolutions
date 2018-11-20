using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

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

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            object obj = NHibernateUtil.String.NullSafeGet(rs, names, session);
            if (obj == null)
            {
                return null;
            }
            return IPAddress.Parse(obj.ToString());
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value == null)
            {
                (cmd.Parameters[index] as IDataParameter).Value = DBNull.Value;
            }
            else
            {
                (cmd.Parameters[index] as IDataParameter).Value = value.ToString();
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

        public SqlType[] SqlTypes => new SqlType[] { SqlTypeFactory.GetString(40) };

        public Type ReturnedType => typeof(IPAddress);

        public bool IsMutable { get; private set; }
    }

    public class PostgresRosterVector : IUserType
    {
        bool IUserType.Equals(object x, object y) => x.Equals(y);
        public int GetHashCode(object x) => x?.GetHashCode() ?? 0;
        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var index = rs.GetOrdinal(names[0]);

            if (rs.IsDBNull(index))
                return RosterVector.Empty;

            RosterVector res = rs.GetValue(index) as int[];

            return res == null ? RosterVector.Empty : res;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = ((IDbDataParameter)cmd.Parameters[index]);
            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                var list = ((RosterVector)value).ToList();
                parameter.Value = list;
            }
        }

        public object DeepCopy(object value) => value;
        public object Replace(object original, object target, object owner) => original;
        public object Assemble(object cached, object owner) => cached;
        public object Disassemble(object value) => value;
        public SqlType[] SqlTypes => new SqlType[] {new NpgsqlExtendedSqlType(DbType.Object, NpgsqlDbType.Array | NpgsqlDbType.Integer)};
        public virtual Type ReturnedType => typeof(RosterVector);
        public bool IsMutable { get; private set; }
    }

    public class PostgresSqlArrayType<T> : IUserType
    {
        bool IUserType.Equals(object x, object y)
        {
            var src = x as T[];
            var dest = y as T[];

            if (src == null && dest == null)
                return true;

            if (src == null || dest == null)
                return false;

            return src.SequenceEqual(dest);
        }

        public int GetHashCode(object x)
        {
            return x?.GetHashCode() ?? 0;
        }

        public virtual object NullSafeGet(DbDataReader resultSet, string[] names, ISessionImplementor session, object owner)
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

        public virtual void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
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
                        this.NpgSqlType)
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

    public interface ITypeConvertor
    {
        object Convert(object b);
    }

    public class PostgresSqlConvertorType<T, TResult, TConvertor> : PostgresSqlArrayType<T>, IUserType where TConvertor: ITypeConvertor
    {
        private readonly ITypeConvertor convertor;

        public PostgresSqlConvertorType()
        {
            this.convertor = Activator.CreateInstance<TConvertor>();
        }

        bool IUserType.Equals(object x, object y)
        {
            return x?.Equals(y) ?? false;
        }

        public new int GetHashCode(object x)
        {
            return x?.GetHashCode() ?? 0;
        }

        public override object NullSafeGet(DbDataReader resultSet, string[] names, ISessionImplementor session, object owner)
        {
            var index = resultSet.GetOrdinal(names[0]);

            if (resultSet.IsDBNull(index))
            {
                return null;
            }

            var value = resultSet.GetValue(index);
            var res = (TResult)convertor.Convert(value);

            if (res != null)
            {
                return res;
            }

            throw new NotImplementedException();
        }

        public override void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = (IDbDataParameter)cmd.Parameters[index];
            if (value == null)
            {
                parameter.Value = DBNull.Value;
            }
            else
            {
                parameter.Value = (T[])this.convertor.Convert(value);
            }
        }
    }

    public abstract class PostgresEntityJson<T> : IUserType where T : class
    {
        protected IEntitySerializer<T> JsonConvert { get; } = new EntitySerializer<T>();

        public new bool Equals(object x, object y)
        {
            var src = x as T;
            var dest = y as T;

            if (src == null && dest == null)
                return true;

            if (src == null || dest == null)
                return false;

            var xdocX = JsonConvert.Serialize(src);
            var xdocY = JsonConvert.Serialize(dest);

            return xdocY == xdocX;
        }

        public int GetHashCode(object x) => x == null ? 0 : x.GetHashCode();
        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            if (names.Length != 1)
                throw new InvalidOperationException("Only expecting one column...");

            var val = rs[names[0]] as string;

            var result = !string.IsNullOrWhiteSpace(val) ? this.JsonConvert.Deserialize(val) : null;
            return result;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var expectedValue = value as T;

            var parameter = (NpgsqlParameter)cmd.Parameters[index];
            parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;

            if (expectedValue == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = JsonConvert.Serialize(expectedValue); 
        }

        public object DeepCopy(object value)
        {
            var expectedValue = value as T;
            if (expectedValue == null)
                return null;

            var serialized = JsonConvert.Serialize(expectedValue);
            return JsonConvert.Deserialize(serialized);
        }

        public object Replace(object original, object target, object owner) => original;

        public object Assemble(object cached, object owner)
        {
            var str = cached as string;
            return string.IsNullOrWhiteSpace(str) ? null : JsonConvert.Deserialize(str);
        }

        public object Disassemble(object value)
        {
            var expectedValue = value as T;
            return expectedValue == null ? null : JsonConvert.Serialize(expectedValue);
        }

        public SqlType[] SqlTypes => new SqlType[] { new NpgsqlExtendedSqlType(DbType.Object, NpgsqlTypes.NpgsqlDbType.Jsonb) };

        public Type ReturnedType => typeof(T);

        public bool IsMutable => true;
    }

    public class PostgresJson<T> : IUserType where T : class
    {
        private IInterviewAnswerSerializer JsonConvert { get; } = new NewtonInterviewAnswerJsonSerializer();

        public new bool Equals(object x, object y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            var xdocX = JsonConvert.Serialize(x);
            var xdocY = JsonConvert.Serialize(y);

            return xdocY == xdocX;
        }

        public int GetHashCode(object x) => x == null ? 0 : x.GetHashCode();
        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            if (names.Length != 1)
                throw new InvalidOperationException("Only expecting one column...");

            var val = rs[names[0]] as string;

            var result = !string.IsNullOrWhiteSpace(val) ? this.JsonConvert.Deserialize<T>(val) : null;
            return result;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = (NpgsqlParameter)cmd.Parameters[index];
            parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;

            if (value == null)
                parameter.Value = DBNull.Value;
            else
                parameter.Value = JsonConvert.Serialize(value);
        }

        public object DeepCopy(object value)
        {
            if (value == null)
                return null;

            var serialized = JsonConvert.Serialize(value);
            return JsonConvert.Deserialize<T>(serialized);
        }

        public object Replace(object original, object target, object owner) => original;

        public object Assemble(object cached, object owner)
        {
            var str = cached as string;
            return string.IsNullOrWhiteSpace(str) ? null : JsonConvert.Deserialize<T>(str);
        }

        public object Disassemble(object value) => value == null ? null : JsonConvert.Serialize(value);

        public SqlType[] SqlTypes => new SqlType[] { new NpgsqlExtendedSqlType(DbType.Object, NpgsqlTypes.NpgsqlDbType.Jsonb) };

        public Type ReturnedType => typeof(T);

        public bool IsMutable => true;
    }

    public class NpgsqlLowerCaseNameTranslator : INpgsqlNameTranslator
    {
        public string TranslateTypeName(string clrName) => ToLower(clrName);
        public string TranslateMemberName(string clrName) => ToLower(clrName);

        private static string ToLower(string clrName) => clrName.ToLowerInvariant();
    }
}

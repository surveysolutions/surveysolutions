using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Infrastructure.Native.Storage.Postgre.NhExtensions
{
    public class PostgreSQL91Dialect : PostgreSQL83Dialect
    {
        public PostgreSQL91Dialect()
        {
            this.RegisterColumnType(DbType.Object, "text[]");
            this.RegisterColumnType(DbType.Object, "numeric[]");
            this.RegisterColumnType(DbType.Object, "integer[]");
            this.RegisterColumnType(DbType.Object, "bigint[]");
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

            if (resultSet.GetValue(index) is T[] res)
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
            { typeof(int[]), NpgsqlDbType.Array | NpgsqlDbType.Integer },
            { typeof(long[]), NpgsqlDbType.Array | NpgsqlDbType.Bigint }
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

    public class PostgresJson<T> : IUserType where T : class
    {
        private static IInterviewAnswerSerializer JsonConvert { get; } = new NewtonInterviewAnswerJsonSerializer();

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

            var result = !string.IsNullOrWhiteSpace(val) ? JsonConvert.Deserialize<T>(val) : null;
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

    /// <summary>
	/// Maps a <see cref="System.DateTimeOffset" /> Property to a <see cref="DateTimeOffset"/>
	/// </summary>
	[Serializable]
    public class DateTimeOffsetType : NHibernate.Type.DateTimeOffsetType
    {
        public override object Get(DbDataReader rs, int index, ISessionImplementor session)
        {
            try
            {
                return DateTimeOffset.Parse(rs.GetString(index));
            }
            catch (Exception ex)
            {
                throw new FormatException($"Input '{rs[index]}' was not in the correct format.", ex);
            }
        }
    }

    public class TimeSpanType : PrimitiveType
    {
        public TimeSpanType() : base(new SqlType(DbType.Object))
        {
        }

        public override string Name { get; } = "TimeSpan";
        public override Type ReturnedClass { get; } = typeof(TimeSpan);

        public override void Set(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            cmd.Parameters[index].Value = value;
        }

        public override object Get(DbDataReader rs, int index, ISessionImplementor session) => rs[index];

        public override object Get(DbDataReader rs, string name, ISessionImplementor session) => rs[name];

        public override string ObjectToSQLString(object value, Dialect dialect) => $"interval '{value:G}'";

        public override Type PrimitiveClass { get; } = typeof(TimeSpan);
        public override object DefaultValue { get; } = TimeSpan.Zero;
    }
}

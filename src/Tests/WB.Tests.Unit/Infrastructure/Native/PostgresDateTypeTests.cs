using System;
using System.Data;
using System.Data.Common;
using Moq;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Tests.Unit.Infrastructure.Native;

[TestOf(typeof(PostgresDateType))]
public class PostgresDateTypeTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void ShouldReadDateOnlyAndLegacyDateTime(bool returnsDateOnly)
    {
        object value = returnsDateOnly
            ? new DateOnly(2026, 9, 24)
            : new DateTime(2026, 9, 24, 13, 45, 56);
        var reader = CreateReader(value);

        var result = new PostgresDateType().NullSafeGet(reader, "date", null);

        Assert.That(result, Is.EqualTo(new DateTime(2026, 9, 24)));
    }

    [Test]
    public void ShouldReadDateOnlyByOrdinal()
    {
        var reader = CreateReader(new DateOnly(2026, 9, 24));

        var result = new PostgresDateType().Get(reader, 0, null);

        Assert.That(result, Is.EqualTo(new DateTime(2026, 9, 24)));
    }

    [Test]
    public void ShouldReadNull()
    {
        var reader = CreateReader(DBNull.Value);

        var result = new PostgresDateType().NullSafeGet(reader, "date", null);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ShouldMapDateTimeToSqlDate()
    {
        var type = new PostgresDateType();

        Assert.That(type.ReturnedClass, Is.EqualTo(typeof(DateTime)));
        Assert.That(type.SqlTypes(null)[0].DbType, Is.EqualTo(DbType.Date));
    }

    private static DbDataReader CreateReader(object value)
    {
        var reader = new Mock<DbDataReader>();
        reader.Setup(x => x.GetOrdinal("date")).Returns(0);
        reader.Setup(x => x[0]).Returns(value);
        reader.Setup(x => x.IsDBNull(0)).Returns(value == DBNull.Value);
        return reader.Object;
    }
}


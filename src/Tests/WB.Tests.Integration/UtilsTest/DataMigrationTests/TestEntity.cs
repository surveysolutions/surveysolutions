using System.ComponentModel.DataAnnotations;

namespace WB.Tests.Integration.UtilsTest.DataMigrationTests
{
    internal class TestEntity
    {
        [Key]
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
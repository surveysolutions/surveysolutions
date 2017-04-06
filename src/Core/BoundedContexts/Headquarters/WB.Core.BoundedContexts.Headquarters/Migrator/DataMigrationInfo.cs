using System.ComponentModel.DataAnnotations;

namespace WB.Core.BoundedContexts.Headquarters.Migrator
{
    public class DataMigrationInfo
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
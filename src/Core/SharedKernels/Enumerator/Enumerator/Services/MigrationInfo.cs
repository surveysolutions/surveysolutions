using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public class MigrationInfo : IMigrationInfo
    {
        private readonly Lazy<IMigration> lazyMigration;

        public MigrationInfo(long version, IMigration migration)
            : this(version, null, () => migration)
        {
        }

        public MigrationInfo(long version, string description, Func<IMigration> migrationFunc)
        {
            if (migrationFunc == null) throw new ArgumentNullException(nameof(migrationFunc));

            Version = version;
            Description = description;
            lazyMigration = new Lazy<IMigration>(migrationFunc);
        }

        public long Version { get; }
        public string Description { get; }
        public IMigration Migration => lazyMigration.Value;

        public string GetName() => $"{Version}: {Migration.GetType().Name}";
        public override string ToString() => $"MigrationType: {Migration.GetType()}";
    }
}

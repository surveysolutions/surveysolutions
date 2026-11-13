using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Quartz
{
    [Migration(2026_11_13_16_00)]
    public class M202611131600_UpgradeQuartzTo47 : ForwardOnlyMigration
    {
        public override void Up()
        {
            // Quartz.NET 4.0-4.7 PostgreSQL upgrade scripts, adapted to the quartz schema.
            Execute.Sql(@"
                ALTER TABLE quartz.triggers
                    ADD COLUMN IF NOT EXISTS misfire_orig_fire_time bigint NULL,
                    ADD COLUMN IF NOT EXISTS execution_group varchar(200) NULL,
                    ADD COLUMN IF NOT EXISTS preferred_node varchar(200) NULL,
                    ADD COLUMN IF NOT EXISTS preferred_node_auto bool NOT NULL DEFAULT false,
                    ADD COLUMN IF NOT EXISTS retry_policy varchar(250) NULL,
                    ADD COLUMN IF NOT EXISTS retry_attempt integer NULL,
                    ADD COLUMN IF NOT EXISTS continues_trigger_name text NULL,
                    ADD COLUMN IF NOT EXISTS continues_trigger_group text NULL,
                    ADD COLUMN IF NOT EXISTS continuation_condition integer NULL,
                    ADD COLUMN IF NOT EXISTS overlap_policy integer NULL,
                    ADD COLUMN IF NOT EXISTS pause_reason varchar(250) NULL,
                    ADD COLUMN IF NOT EXISTS paused_by varchar(200) NULL,
                    ADD COLUMN IF NOT EXISTS paused_at bigint NULL,
                    ADD COLUMN IF NOT EXISTS misfire_threshold bigint NULL,
                    ADD COLUMN IF NOT EXISTS fixed_delay bool NULL;

                ALTER TABLE quartz.fired_triggers
                    ADD COLUMN IF NOT EXISTS execution_group varchar(200) NULL,
                    ADD COLUMN IF NOT EXISTS progress integer NULL,
                    ADD COLUMN IF NOT EXISTS progress_message varchar(250) NULL,
                    ADD COLUMN IF NOT EXISTS interrupt_requested_time bigint NULL;

                CREATE TABLE IF NOT EXISTS quartz.paused_job_grps (
                    sched_name text NOT NULL,
                    job_group text NOT NULL,
                    PRIMARY KEY (sched_name, job_group)
                );

                ALTER TABLE quartz.paused_job_grps
                    ADD COLUMN IF NOT EXISTS pause_reason varchar(250) NULL,
                    ADD COLUMN IF NOT EXISTS paused_by varchar(200) NULL,
                    ADD COLUMN IF NOT EXISTS paused_at bigint NULL;

                ALTER TABLE quartz.paused_trigger_grps
                    ADD COLUMN IF NOT EXISTS pause_reason varchar(250) NULL,
                    ADD COLUMN IF NOT EXISTS paused_by varchar(200) NULL,
                    ADD COLUMN IF NOT EXISTS paused_at bigint NULL;

                ALTER TABLE quartz.scheduler_state
                    ADD COLUMN IF NOT EXISTS quartz_version varchar(20) NULL;
            ");
        }
    }
}

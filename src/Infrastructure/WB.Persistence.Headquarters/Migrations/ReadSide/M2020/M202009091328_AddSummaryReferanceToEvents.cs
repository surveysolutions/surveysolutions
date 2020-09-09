using System.Data;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    /*[Migration(202009091328)]
    public class M202009091328_AddSummaryReferanceToEvents : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                CREATE unique INDEX interviewsummaries_interviewid_unique_idx ON readside.interviewsummaries USING btree (interviewid)

                ALTER TABLE events.events
                ADD CONSTRAINT fk_summary
                    FOREIGN KEY (id)
                REFERENCES readside.interviewsummaries(interviewid)
                ON DELETE CASCADE;"
            );
            /*
            Create.ForeignKey("fk_summary_to_events")
                .FromTable("events").InSchema("events").ForeignColumn("id")
                .ToTable("interviewsummaries").InSchema("readside").PrimaryColumn("interviewid")
                .OnDelete(Rule.Cascade);
            #1#
            /*
            Create.ForeignKey("fk_summary_to_events")
                .FromTable("interviewsummaries").InSchema("readside").ForeignColumn("interviewid")
                .ToTable("events").InSchema("events").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        #1#
        }

        public override void Down()
        {
            
        }
    }*/
}

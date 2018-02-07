using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(201802121510)]
    public class M201802121510_AddIdToInterviewSummaryId : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE readside.interviewsummaries ADD id serial NOT NULL");
            Execute.Sql(@"CREATE UNIQUE INDEX if not exists interviewsummaries_id_idx ON readside.interviewsummaries USING btree (id);");
            Execute.Sql(@"CREATE UNIQUE INDEX if not exists interviewsummaries_interviewid_idx ON readside.interviewsummaries USING btree (interviewid);");
        }

        public override void Down()
        {
            Execute.Sql(@"ALTER TABLE readside.interviewsummaries DROP COLUMN id");
        }
    }
}
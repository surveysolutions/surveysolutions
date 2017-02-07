using System.Text;
using FluentMigrator;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    [Migration(8)]
    public class M008_AddInterviewHumanId : Migration
    {

        public override void Up()
        {
            this.Create.Column(@"humanid").OnTable(@"interviewsummaries").AsInt32().Nullable();

            var script = new StringBuilder();
            script.AppendLine(@"CREATE OR REPLACE FUNCTION readside.pseudo_encrypt(VALUE bigint) returns int AS $$");
            script.AppendLine(@"DECLARE");
            script.AppendLine(@"l1 int;");
            script.AppendLine(@"l2 int;");
            script.AppendLine(@"r1 int;");
            script.AppendLine(@"r2 int;");
            script.AppendLine(@"i int:= 0;");
            script.AppendLine(@"BEGIN");
            script.AppendLine(@"l1:= (VALUE >> 16) & 65535;");
            script.AppendLine(@"r1:= VALUE & 65535;");
            script.AppendLine(@"WHILE i < 3 LOOP");
            script.AppendLine(@"l2 := r1;");
            script.AppendLine(@"r2:= l1 # ((((1366 * r1 + 150889) % 714025) / 714025.0) * 32767)::int;");
            script.AppendLine(@"l1:= l2;");
            script.AppendLine(@"r1:= r2;");
            script.AppendLine(@"i:= i + 1;");
            script.AppendLine(@"END LOOP;");
            script.AppendLine(@"RETURN((r1 << 16) + l1);");
            script.AppendLine(@"END;");
            script.AppendLine(@"$$ LANGUAGE plpgsql strict immutable;");
            script.AppendLine(@"CREATE SEQUENCE readside.interview_humanid_seq INCREMENT BY 1;");
            script.AppendLine(@"CREATE FUNCTION readside.trigger_interviewsummaries_before_insert () RETURNS trigger AS $$");
            script.AppendLine(@"BEGIN");
            script.AppendLine(@"NEW.humanid = readside.pseudo_encrypt(nextval('readside.interview_humanid_seq'));");
            script.AppendLine(@"return NEW;");
            script.AppendLine(@"END;");
            script.AppendLine(@"$$ LANGUAGE  plpgsql;");
            script.AppendLine(@"CREATE TRIGGER interviewsummaries_before_insert");
            script.AppendLine(@"BEFORE INSERT ON readside.interviewsummaries FOR EACH ROW ");
            script.AppendLine(@"EXECUTE PROCEDURE readside.trigger_interviewsummaries_before_insert ()");

            this.Execute.Sql(script.ToString());
        }

        public override void Down()
        {
            this.Delete.Column(@"humanid").FromTable(@"interviewsummaries");

            var script = new StringBuilder();
            script.AppendLine(@"DROP FUNCTION readside.pseudo_encrypt(bigint);");
            script.AppendLine(@"DROP SEQUENCE readside.interview_humanid_seq;");
            script.AppendLine(@"DROP TRIGGER interviewsummaries_before_insert ON readside.interviewsummaries;");
            script.AppendLine(@"DROP FUNCTION readside.trigger_interviewsummaries_before_insert();");

            this.Execute.Sql(script.ToString());
        }
    }
}
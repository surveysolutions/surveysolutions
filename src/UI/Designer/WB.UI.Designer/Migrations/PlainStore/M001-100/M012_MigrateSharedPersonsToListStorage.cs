using System;
using System.Collections.Generic;
using System.Dynamic;
using FluentMigrator;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(12)]
    public class M012_MigrateSharedPersonsToListStorage : Migration
    {
        private const string sharedPersonsTableName = "sharedpersons";
        private const string archivePrefix = "zz_archived_";

        public override void Up()
        {
            List<string> questionnaireIds = new List<string>();

            Rename.Table(sharedPersonsTableName).To(archivePrefix + sharedPersonsTableName);
            Create.Table(sharedPersonsTableName)
                .WithColumn("questionnaireid").AsString().NotNullable()
                .WithColumn("userid").AsGuid().NotNullable()
                .WithColumn("email").AsString().NotNullable()
                .WithColumn("isowner").AsBoolean().NotNullable()
                .WithColumn("sharetype").AsInt16().NotNullable();
            Create.Index("questionnairelistviewitem_sharedpersons_1").OnTable(sharedPersonsTableName)
                .OnColumn("questionnaireid").Ascending();

            Execute.WithConnection((con, trans) =>
            {
                var dbCommand = con.CreateCommand();
                dbCommand.CommandText = "select id from plainstore.questionnairelistviewitems";
                using (var reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questionnaireIds.Add(reader[0] as string);
                    }
                }

                foreach (var questionnaireId in questionnaireIds)
                {
                    var shareDataCommand = con.CreateCommand();
                    shareDataCommand.CommandText =
@"select a.sp ->> 'Id' as userId, a.sp ->> 'Email' as email, a.sp ->> 'IsOwner' as isOwner, a.sp->>'ShareType' as shareType 
    from(select json_array_elements(value-> 'SharedPersons') as sp
        from plainstore.questionnairesharedpersons
        where id = :questionnaireId) a ";
                    var dbDataParameter = shareDataCommand.CreateParameter();
                    dbDataParameter.ParameterName = "questionnaireId";
                    dbDataParameter.Value = questionnaireId;
                    shareDataCommand.Parameters.Add(dbDataParameter);

                    List<dynamic> shareData = new List<dynamic>();
                    using (var reader = shareDataCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userId = reader["userId"] as string;
                            var email = reader["email"] as string;
                            var isOwner = reader["isOwner"] as string == "true";
                            var shareType = reader["shareType"] as string == "1" ? 1 : 0;

                            dynamic shareItem = new ExpandoObject();
                            shareItem.userId = userId;
                            shareItem.email = email;
                            shareItem.isOwner = isOwner;
                            shareItem.shareType = shareType;
                            shareData.Add(shareItem);
                        }
                    }

                    foreach (var shareItem in shareData)
                    {
                        var insertCommand = con.CreateCommand();
                        insertCommand.CommandText =
                            $"INSERT INTO plainstore.\"{sharedPersonsTableName}\" (questionnaireid, userid, email, isowner, sharetype) VALUES (:questionnaireid, :userid, :email, :isowner, :sharetype)";

                        var questionnaireIdParam = insertCommand.CreateParameter();
                        questionnaireIdParam.ParameterName = "questionnaireid";
                        questionnaireIdParam.Value = questionnaireId;

                        var userIdParam = insertCommand.CreateParameter();
                        userIdParam.ParameterName = "userid";
                        userIdParam.Value = Guid.Parse(shareItem.userId);

                        var emailParam = insertCommand.CreateParameter();
                        emailParam.ParameterName = "email";
                        emailParam.Value = shareItem.email;

                        var isownerParam = insertCommand.CreateParameter();
                        isownerParam.ParameterName = "isowner";
                        isownerParam.Value = shareItem.isOwner;

                        var shareTypeParam = insertCommand.CreateParameter();
                        shareTypeParam.ParameterName = "sharetype";
                        shareTypeParam.Value = shareItem.shareType;

                        insertCommand.Parameters.Add(questionnaireIdParam);
                        insertCommand.Parameters.Add(userIdParam);
                        insertCommand.Parameters.Add(emailParam);
                        insertCommand.Parameters.Add(isownerParam);
                        insertCommand.Parameters.Add(shareTypeParam);

                        insertCommand.ExecuteNonQuery();
                    }
                }
            });

            Rename.Table("questionnairesharedpersons").To($"{archivePrefix}questionnairesharedpersons");
        }

        public override void Down()
        {
            Rename.Table($"{archivePrefix}questionnairesharedpersons").To("questionnairesharedpersons");
            Delete.Table(sharedPersonsTableName);
            Rename.Table(archivePrefix + sharedPersonsTableName).To(sharedPersonsTableName);
        }
    }
}
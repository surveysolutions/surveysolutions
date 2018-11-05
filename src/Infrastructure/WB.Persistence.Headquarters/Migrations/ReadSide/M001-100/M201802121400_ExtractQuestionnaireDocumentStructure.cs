using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;

namespace WB.Persistence.Headquarters.Migrations.ReadSide
{
    [Migration(201802121400)]
    [Localizable(false)]
    public class M201802121400_ExtractQuestionnaireDocumentStructure : Migration
    {
        public override void Up()
        {
            const string schema = "readside"; // will be moved to another namespace in later migration. Slow interviews migration cross schema
            // migrator don't want to create table in plainstore schema
            Execute.Sql($@"create
                table
                    {schema}.questionnaire_entities(
                        id serial not null,
                        questionnaireidentity varchar(255) not null,
                        entityid uuid not null,
                        parentid uuid,
                        question_type integer,
                        featured boolean,
                        question_scope integer,
                        entity_type int,
                        constraint PK_questionnaire_entities primary key(id));");

            Execute.Sql($@"
                CREATE INDEX questionnaire_entities_questionnaireid_idx ON {schema}.questionnaire_entities (questionnaireidentity);
                CREATE INDEX questionnaire_entities_entityid_idx ON {schema}.questionnaire_entities (entityid);");

            Execute.Sql(@"ALTER TABLE readside.questionnaire_entities ADD CONSTRAINT questionnaire_entities_un UNIQUE (questionnaireidentity,entityid)");

            Execute.WithConnection((db, dt) =>
            {
                if(string.IsNullOrWhiteSpace(db.QuerySingle<string>("SELECT to_regclass('plainstore.questionnairedocuments')::text")))
                    return;

                int limit = 5;
                int skip = 0;
                bool isExistsDocuments = true;

                do
                {
                    var documentRows = db.Query<(string id, string value)>($"select id, value from plainstore.questionnairedocuments order by id limit {limit} OFFSET {skip}").ToList();

                    foreach (var documentRow in documentRows)
                    {
                        var docId = documentRow.id;
                        var doc = JObject.Parse(documentRow.value);

                        IEnumerable<QuestionnaireEntity> ExtractEntities(JObject item, JObject parent)
                        {
                            if (item["Children"] is JArray childs)
                            {
                                foreach (var child in childs)
                                {
                                    if (child is JObject childItem)
                                    {
                                        foreach (var entity in ExtractEntities(childItem, item))
                                        {
                                            yield return entity;
                                        }
                                    }
                                }
                            }

                            if (parent != null)
                            {
                                var parentKey = parent["PublicKey"].Value<string>();

                                var type = item["$type"]?.Value<string>();
                                EntityType? entityType = type == null ? (EntityType?) null : EntityMap[type];

                                var entity = new QuestionnaireEntity
                                {
                                    EntityId = Guid.Parse(item["PublicKey"].Value<string>()),
                                    ParentId = Guid.Parse(parentKey),
                                    QuestionType = (QuestionType?) item["QuestionType"]?.Value<long>(),
                                    QuestionnaireIdentity = docId,
                                    Featured = item["Featured"]?.Value<bool>(),
                                    QuestionScope = (QuestionScope?) item["QuestionScope"]?.Value<long>() ?? 0,
                                    EntityType = entityType
                                };

                                yield return entity;
                            }
                        }

                        var list = ExtractEntities(doc, null).ToList();

                        foreach (var entity in list)
                        {
                            db.Execute(
                                $@"insert into {schema}.questionnaire_entities 
                                    (questionnaireidentity, entityid, parentid, question_type, featured, question_scope, entity_type)
                                values(
                                    @QuestionnaireIdentity, @EntityId, @ParentId, @QuestionType, @Featured, @QuestionScope, @EntityType)",
                                entity);
                        }
                    }

                    skip += limit;
                    isExistsDocuments = documentRows.Count >= limit;

                } while (isExistsDocuments);
            });
        }

        private enum QuestionScope { Interviewer = 0, Supervisor = 1, Headquarter = 2, Hidden = 3 }

        private enum QuestionType
        {
            SingleOption = 0,
            [Obsolete("db contains at least one questionnaire")]
            YesNo = 1,
            MultyOption = 3,
            Numeric = 4,
            DateTime = 5,
            GpsCoordinates = 6,
            Text = 7,
            [Obsolete("db contains a bunch of them")]
            AutoPropagate = 8,
            TextList = 9,
            QRBarcode = 10,
            Multimedia = 11,
            Area = 12,
            Audio = 13
        }

        private class QuestionnaireEntity
        {
            public string QuestionnaireIdentity { get; set; }
            public Guid EntityId { get; set; }
            public Guid? ParentId { get; set; }
            public QuestionType? QuestionType { get; set; }
            public bool? Featured { get; set; }
            public QuestionScope? QuestionScope { get; set; }
            public EntityType? EntityType { get; set; }
        }

        private enum EntityType
        {
            Section = 1,
            Question = 2,
            StaticText = 3,
            Variable = 4
        }

        private static readonly Dictionary<string, EntityType> EntityMap = new Dictionary<string, EntityType>
        {
            {"QuestionnaireDocument", EntityType.Section},
            {"Group", EntityType.Section},
            {"StaticText", EntityType.StaticText},
            {"AreaQuestion", EntityType.Question},
            {"AudioQuestion", EntityType.Question},
            {"DateTimeQuestion", EntityType.Question},
            {"GpsCoordinateQuestion", EntityType.Question},
            {"MultimediaQuestion", EntityType.Question},
            {"MultyOptionsQuestion", EntityType.Question},
            {"NumericQuestion", EntityType.Question},
            {"QRBarcodeQuestion", EntityType.Question},
            {"SingleQuestion", EntityType.Question},
            {"TextListQuestion", EntityType.Question},
            {"TextQuestion", EntityType.Question},
            {"Variable", EntityType.Variable}
        };

        public override void Down()
        {
            Delete.Table("questionnaire_entities").InSchema("readside");
        }
    }
}

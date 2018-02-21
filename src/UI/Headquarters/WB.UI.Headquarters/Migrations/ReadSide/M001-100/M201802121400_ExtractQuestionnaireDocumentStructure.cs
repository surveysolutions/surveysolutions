﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;

namespace WB.UI.Headquarters.Migrations.ReadSide
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
                        variable_name text,
                        question_type integer,
                        featured boolean,
                        question_scope integer,
                        type text,
                        constraint PK_questionnaire_entities primary key(id));");

            Execute.Sql($@"
                CREATE INDEX questionnaire_entities_questionnaireid_idx ON {schema}.questionnaire_entities (questionnaireidentity);
                CREATE INDEX questionnaire_entities_entityid_idx ON {schema}.questionnaire_entities (entityid);");

            Execute.Sql(@"ALTER TABLE readside.questionnaire_entities ADD CONSTRAINT questionnaire_entities_un UNIQUE (questionnaireidentity,entityid)");

            Execute.WithConnection((db, dt) =>
            {
                if(string.IsNullOrWhiteSpace(db.QuerySingle<string>("SELECT to_regclass('plainstore.questionnairedocuments')::text")))
                    return;

                foreach (var documentRow in db.Query<(string id, string value)>(@"select id, value from plainstore.questionnairedocuments"))
                {
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

                            var entity = new QuestionnaireEntity
                            {
                                EntityId = Guid.Parse(item["PublicKey"].Value<string>()),
                                ParentId = Guid.Parse(parentKey),
                                QuestionType = (QuestionType?) item["QuestionType"]?.Value<long>(),
                                QuestionnaireIdentity = documentRow.id,
                                VariableName = item["StataExportCaption"]?.Value<string>(),
                                Featured = item["Featured"]?.Value<bool>(),
                                QuestionScope = (QuestionScope?) item["QuestionScope"]?.Value<long>() ?? 0,
                                Type = item["$type"]?.Value<string>()
                            };

                            yield return entity;
                        }
                    }

                    var list = ExtractEntities(doc, null).ToList();

                    foreach (var entity in list)
                    {
                        db.Execute(
                            $@"insert into {schema}.questionnaire_entities 
                                (questionnaireidentity, entityid, parentid, question_type, variable_name, featured, question_scope, type)
                            values(
                                @QuestionnaireIdentity, @EntityId, @ParentId, @QuestionType, @VariableName, @Featured, @QuestionScope, @Type)",
                            entity);
                    }
                }
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
            public string VariableName { get; set; }
            public bool? Featured { get; set; }
            public QuestionScope? QuestionScope { get; set; }
            public string Type { get; set; }
        }

        public override void Down()
        {
            Delete.Table("questionnaire_entities").InSchema("readside");
        }
    }
}
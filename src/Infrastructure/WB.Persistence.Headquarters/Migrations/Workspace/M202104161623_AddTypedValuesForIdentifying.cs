using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using FluentMigrator;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json.Linq;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Persistence.Headquarters.Migrations.Workspace
{
    [Migration(202104161623)]
    public class M202104161623_AddTypedValuesForIdentifying : ForwardOnlyMigration
    {
        public override void Up()
        {
            Execute.WithConnection((db, dt) =>
            {
                if(string.IsNullOrWhiteSpace(db.QuerySingle<string>("SELECT to_regclass('questionnairedocuments')::text")))
                    return;

                int limit = 5;
                int skip = 0;
                bool isExistsDocuments = true;

                do
                {
                    var documentRows = db.Query<(string id, string value)>($"select id, value from questionnairedocuments order by id limit {limit} OFFSET {skip}").ToList();

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
                                    EntityType = entityType,
                                    IsInteger = item["IsInteger"]?.Value<bool>(),
                                    VariableType = (VariableType?) item["Type"]?.Value<long>(),
                                };

                                yield return entity;
                            }
                        }

                        var entitiesList = ExtractEntities(doc, null).ToList();

                        var integerQuestions = entitiesList.Where(x => x.EntityType == EntityType.Question 
                                                                       && x.QuestionType == QuestionType.Numeric 
                                                                       && x.IsInteger == true ).Select(x => x.EntityId).ToList();
                        if (integerQuestions.Count > 0)
                        {
                            db.Execute($@"update identifyingentityvalue                                            
                                            set value_long = cast(value as int)
                                            from questionnaire_entities 
                                            where identifyingentityvalue.entity_id = questionnaire_entities.id
                                            and identifyingentityvalue.value != ''
                                            and questionnaire_entities.questionnaireidentity = '{docId}'
                                            and questionnaire_entities.entityid = Any(@integerQuestions)", new {integerQuestions = integerQuestions});

                        }

                        var realQuestions = entitiesList.Where(x => x.EntityType == EntityType.Question 
                                                                    && x.QuestionType == QuestionType.Numeric 
                                                                    && x.IsInteger == false).Select(x => x.EntityId).ToList();
                        if (realQuestions.Count > 0)
                        {
                            db.Execute($@"update identifyingentityvalue as iev
                                            set iev.value_double = cast(iev.value as float)
                                            from questionnaire_entities as qe 
                                            where iev.entity_id = qe.id
                                            and iev.value != ''
                                            and qe.questionnaireidentity = '{docId}'
                                            and qe.entityid = Any(@realQuestions)", new{ realQuestions = realQuestions});

                        }

                        var dateTimeQuestions = entitiesList.Where(x => x.EntityType == EntityType.Question 
                                                                        && x.QuestionType == QuestionType.DateTime).Select(x => x.EntityId).ToList();
                        if (dateTimeQuestions.Count > 0)
                        {
                            db.Execute($@"update identifyingentityvalue as iev                                            
                                            set value_date = cast(value as timestamp)
                                            from questionnaire_entities as qe 
                                            where iev.entity_id = qe.id
                                            and iev.value != ''
                                            and qe.questionnaireidentity = '{docId}'
                                            and qe.entityid = Any(@dateTimeQuestions)", new{dateTimeQuestions = dateTimeQuestions});
                        }

                        var booleanVariables = entitiesList.Where(x =>x.EntityType == EntityType.Variable 
                                                                      && x.VariableType == VariableType.Boolean).Select(x => x.EntityId).ToList();
                        if (booleanVariables.Count > 0)
                        {
                            db.Execute($@"update identifyingentityvalue as iev
                                            set value_bool = cast(value as boolean)
                                            from questionnaire_entities as qe 
                                            where iev.entity_id = qe.id
                                            and iev.value != ''
                                            and qe.questionnaireidentity = '{docId}'
                                            and qe.entityid = Any(@booleanVariables)", new{booleanVariables = booleanVariables});
                        }
                        var doubleVariables = entitiesList.Where(x =>x.EntityType == EntityType.Variable 
                                                                     && x.VariableType == VariableType.Double).Select(x => x.EntityId).ToList();
                        if (doubleVariables.Count > 0)
                        {
                            db.Execute($@"update identifyingentityvalue as iev
                                            set value_double = cast(value as float)
                                            from questionnaire_entities as qe 
                                            where iev.entity_id = qe.id
                                            and iev.value != ''                                            
                                            and qe.questionnaireidentity = '{docId}'
                                            and qe.entityid = Any(@doubleVariables)", new{doubleVariables = doubleVariables});
                        }
                        var dateTimeVariables = entitiesList.Where(x =>x.EntityType == EntityType.Variable 
                                                                       && x.VariableType == VariableType.DateTime).Select(x => x.EntityId).ToList();
                        if (dateTimeVariables.Count > 0)
                        {
                            db.Execute($@"update identifyingentityvalue as iev
                                            set value_date = cast(value as timestamp)
                                            from questionnaire_entities as qe 
                                            where iev.entity_id = qe.id
                                            and iev.value != ''
                                            and qe.questionnaireidentity = '{docId}'
                                            and qe.entityid = Any(@dateTimeVariables)", new{dateTimeVariables = dateTimeVariables});
                        }
                        var longVariables = entitiesList.Where(x =>x.EntityType == EntityType.Variable 
                                                                   && x.VariableType == VariableType.LongInteger).Select(x => x.EntityId).ToList();
                        if (longVariables.Count > 0)
                        {
                            db.Execute($@"update identifyingentityvalue as iev
                                            set value_long = cast(value as int)
                                            from questionnaire_entities as qe 
                                            where iev.entity_id = qe.id
                                            and iev.value != ''
                                            and qe.questionnaireidentity = '{docId}'
                                            and qe.entityid = Any(@longVariables)", new{longVariables = longVariables});
                        }
                    }

                    skip += limit;
                    isExistsDocuments = documentRows.Count >= limit;

                } while (isExistsDocuments);
            });
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

        private class QuestionnaireEntity
        {
            public string QuestionnaireIdentity { get; set; }
            public Guid EntityId { get; set; }
            public Guid? ParentId { get; set; }
            public QuestionType? QuestionType { get; set; }
            public EntityType? EntityType { get; set; }
            public VariableType? VariableType { get; set; }
            public bool? IsInteger { get; set; }
        }
    }
}

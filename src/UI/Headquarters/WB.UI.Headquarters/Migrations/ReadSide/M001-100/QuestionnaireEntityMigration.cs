using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using Newtonsoft.Json.Linq;

namespace WB.UI.Headquarters.Migrations.ReadSide
{
    public abstract class QuestionnaireEntityMigration : Migration
    {
        [Localizable(false)]
        protected void ExecuteForQuestionnaireItem(Action<IDbConnection, string, JObject, JObject> action)
        {
            Execute.WithConnection((db, dt) =>
            {
                if (string.IsNullOrWhiteSpace(
                    db.QuerySingle<string>("SELECT to_regclass('plainstore.questionnairedocuments')::text")))
                    return;

                var questionnaireList = db.Query<string>("select id from plainstore.questionnairedocuments").ToList();

                foreach (var questionnaireId in questionnaireList)
                {
                    var questionnaireJson = db.QuerySingleOrDefault<string>(
                        $"select value from plainstore.questionnairedocuments where id = '{questionnaireId}'");

                    var json = JObject.Parse(questionnaireJson);

                    var questions = ExtractEntities(json, null);

                    foreach (var question in questions)
                    {
                        action(db, questionnaireId, question.item, question.parent);
                    }
                }
            });
        }

        [Localizable(false)]
        protected IEnumerable<(JObject item, JObject parent)> ExtractEntities(JObject item, JObject parent)
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
                yield return (item, parent);
            }
        }
    }
}
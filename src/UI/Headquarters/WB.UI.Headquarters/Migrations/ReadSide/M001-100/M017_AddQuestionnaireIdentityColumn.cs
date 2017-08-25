using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Dapper;
using FluentMigrator;
using NLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Migrations.ReadSide
{

    [Localizable(false)]
    [Migration(17)]
    public class M017_AddQuestionnaireIdentityColumn : Migration
    {
        private static readonly string InterviewSummariesTable = "interviewsummaries";
        private static readonly string QuestionnaireIdentityColumn = "questionnaireidentity";

        public override void Up()
        {
            Alter.Table(InterviewSummariesTable).AddColumn(QuestionnaireIdentityColumn).AsString().Nullable();
            
            Execute.WithConnection((con, transaction) =>
            {
                List<dynamic> existingInterviewIds =
                    con.Query($"select interviewid, questionnaireid, questionnaireversion from readside.{InterviewSummariesTable}").ToList();
                    
                var currentClassLogger = LogManager.GetLogger(nameof(M017_AddQuestionnaireIdentityColumn));
                currentClassLogger.Info($"Starting add of questionnaireIdentity for interview summaries. Total interviews count: {existingInterviewIds.Count}");
                    
                Stopwatch watch = Stopwatch.StartNew();
                Stopwatch batchWatch = Stopwatch.StartNew();
                for (int i = 0; i < existingInterviewIds.Count; i++)
                {
                    Guid existingInterviewId = existingInterviewIds[i].interviewid;
                    var questionnaireIdentity = new QuestionnaireIdentity(existingInterviewIds[i].questionnaireid, existingInterviewIds[i].questionnaireversion);

                    con.Execute($"UPDATE readside.{InterviewSummariesTable} SET {QuestionnaireIdentityColumn} = @qi WHERE summaryid = @interviewId",
                        new
                        {
                            qi = questionnaireIdentity.ToString(),
                            interviewId = existingInterviewId.FormatGuid()
                        });

                    if (i % 10000 == 0)
                    {
                        currentClassLogger.Info($"Migrated {i} interviews. Batch took: {batchWatch.Elapsed:g}. In tootal: {watch.Elapsed:g}");
                        batchWatch.Restart();
                    }
                }
            });

            Create.Index("interviewsummaries_questionnaire_identity_indx").OnTable(InterviewSummariesTable).OnColumn(QuestionnaireIdentityColumn);
        }

        public override void Down()
        {
            Delete.Column(QuestionnaireIdentityColumn).FromTable(InterviewSummariesTable);
        }
    }
}
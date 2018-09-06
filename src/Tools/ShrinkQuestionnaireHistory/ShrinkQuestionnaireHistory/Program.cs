using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DeltaCompressionDotNet.MsDelta;
using JsonDiffPatchDotNet;
using Npgsql;

namespace ShrinkQuestionnaireHistory
{
    [Localizable(false)]
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = new NpgsqlConnectionStringBuilder(args[0]);

            var compression = new MsDeltaCompression(); 
            using (var connection = new NpgsqlConnection(connectionString.ConnectionString))
            {
                await connection.OpenAsync();

                var questionnairesWithHistoryIds = (await connection.QueryAsync<string>(
                        "SELECT DISTINCT questionnaireid FROM plainstore.questionnairechangerecords WHERE resultingquestionnairedocument IS NOT NULL")
                    ).ToList();

                Console.WriteLine($"Found {questionnairesWithHistoryIds.Count} to migrate");

                int processedCount = 0;
                Stopwatch watch = Stopwatch.StartNew();

                foreach (var row in questionnairesWithHistoryIds)
                {
                    using (var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
                    {
                        try
                        {
                            var existingHistory = connection.Query<(string Id, string Questionnaire)>(
                                @"SELECT id as Id, resultingquestionnairedocument as Questionnaire 
                                          FROM plainstore.questionnairechangerecords 
                                          WHERE questionnaireid = @QuestionnaireId AND resultingquestionnairedocument IS NOT NULL
                                          ORDER BY ""sequence""", new {QuestionnaireId = row});

                            string reference = existingHistory.First().Questionnaire;

                            foreach (var historyItem in existingHistory.Skip(1))
                            {
                                File.WriteAllText("source.json", reference);
                                File.WriteAllText("target.json", historyItem.Questionnaire ?? "");

                                compression.CreateDelta("source.json", "target.json", "delta");

                                var delta = File.ReadAllBytes("delta");

                                connection.Execute(@"UPDATE plainstore.questionnairechangerecords 
                                                                    SET resultingquestionnairedocument = NULL, binarydiff = @diff 
                                                                    WHERE id = @id",
                                    new
                                    {
                                        diff = delta,
                                        id = historyItem.Id
                                    });

                                reference = historyItem.Questionnaire;
                            }

                            transaction.Commit();
                            processedCount++;

                            if (processedCount % 100 == 0)
                            {
                                Console.WriteLine(
                                    $"Processed {processedCount} questionnaires.Elapsed {watch.Elapsed:g}");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            transaction.Rollback();
                        }
                    }
                }
            }

            Console.WriteLine("Done migration");
        }
    }
}

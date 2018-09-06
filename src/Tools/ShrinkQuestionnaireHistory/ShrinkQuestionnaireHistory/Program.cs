using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace ShrinkQuestionnaireHistory
{
    [Localizable(false)]
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = new NpgsqlConnectionStringBuilder(args[0]);

            var patcher = new DiffMatchPatch.diff_match_patch();
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
                                          WHERE questionnaireid = @questionnaireId AND resultingquestionnairedocument IS NOT NULL
                                          ORDER BY ""sequence""", new {row});

                            string reference = existingHistory.First().Questionnaire;

                            foreach (var historyItem in existingHistory.Skip(1))
                            {
                                var diff = patcher.patch_make(reference,
                                    historyItem.Questionnaire ?? string.Empty);
                                string textPatch = patcher.patch_toText(diff);

                                var appliedPatch = patcher.patch_apply(diff, reference)[0].ToString();
                                if (!appliedPatch.Equals(historyItem.Questionnaire))
                                {
                                    throw new Exception(
                                        $"Applied patch for questionnaire {row}. Applied patch does not match target questionnaire. Tested change id {historyItem.Id}");
                                }

                                connection.Execute(@"UPDATE plainstore.questionnairechangerecords 
                                                                    SET resultingquestionnairedocument = NULL, diffwithpreviousversion = @diff 
                                                                    WHERE id = @id",
                                    new
                                    {
                                        diff = textPatch,
                                        id = historyItem.Id
                                    });

                                reference = historyItem.Questionnaire;
                            }

                            transaction.Commit();
                            Interlocked.Increment(ref processedCount);

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

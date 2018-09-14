using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using JsonDiffPatchDotNet;
using Npgsql;

namespace ShrinkQuestionnaireHistory
{
    [Localizable(false)]
    class Program
    {
        public static string CompressString(string stringToCompress)
        {
            var bytes = Encoding.Unicode.GetBytes(stringToCompress);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        private const string emptyJson = "{}";

        static async Task Main(string[] args)
        {
            var connectionString = new NpgsqlConnectionStringBuilder(args[0]);

            var jdp = new JsonDiffPatch();
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
                                          ORDER BY ""sequence"" desc", new { questionnaireId  = row });

                            string reference = existingHistory.First().Questionnaire;

                            foreach (var historyItem in existingHistory.Skip(1))
                            {
                                string textPatch = jdp.Diff(reference ?? emptyJson, historyItem.Questionnaire ?? emptyJson);

                                var compressString = CompressString(textPatch);
                                connection.Execute(@"UPDATE plainstore.questionnairechangerecords 
                                                    SET resultingquestionnairedocument = NULL, ""patch"" = @diff 
                                                    WHERE id = @id",
                                    new
                                    {
                                        diff = compressString,
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

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

        public static string DecompressString(string compressedString)
        {
            var bytes = Convert.FromBase64String(compressedString);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
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

                Parallel.ForEach(questionnairesWithHistoryIds, new ParallelOptions{MaxDegreeOfParallelism = 4}, questionnaireId =>
                {
                    using (var localConnection = new NpgsqlConnection(connectionString.ConnectionString))
                    {
                        localConnection.Open();
                        using (var transaction = localConnection.BeginTransaction(IsolationLevel.RepeatableRead))
                        {
                            try
                            {
                                var existingHistory =
                                    localConnection
                                        .Query<(string Id, string questionnaireId, string Questionnaire, string patch)>(
                                            @"SELECT id as Id, questionnaireId, resultingquestionnairedocument, patch as Questionnaire 
                                          FROM plainstore.questionnairechangerecords 
                                          WHERE questionnaireid = :questionnaireId AND (resultingquestionnairedocument IS NOT NULL OR patch IS NOT NULL)
                                          ORDER BY ""sequence"" desc", new {questionnaireId = questionnaireId}, transaction);

                                string reference = existingHistory.First().Questionnaire;

                                foreach (var historyItem in existingHistory.Skip(1))
                                {
                                    if (historyItem.Questionnaire != null)
                                    {

                                        string textPatch = jdp.Diff(reference ?? emptyJson,
                                            historyItem.Questionnaire ?? emptyJson);


                                        var compressString = textPatch != null ? CompressString(textPatch) : null;
                                        localConnection.Execute(@"UPDATE plainstore.questionnairechangerecords 
                                                    SET resultingquestionnairedocument = NULL, ""patch"" = @diff 
                                                    WHERE id = @id",
                                            new
                                            {
                                                diff = compressString,
                                                id = historyItem.Id
                                            });

                                        reference = historyItem.Questionnaire;
                                    }
                                    else
                                    {
                                        var patch = DecompressString(historyItem.patch);
                                        reference = jdp.Patch(reference, patch);
                                    }
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
                });
            }

            Console.WriteLine("Done migration");
        }
    }
}

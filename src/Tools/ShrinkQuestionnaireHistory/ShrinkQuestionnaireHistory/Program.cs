using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Fclp;
using Npgsql;

namespace ShrinkQuestionnaireHistory
{
    public class ApplicationArguments
    {
        public string ConnectionString { get; set; }

    }

    public class ChangeRecord
    {
        public string Id { get; set; }

        public string Questionnaire { get; set; }
    }

    public class QuestionnaireIdRecord
    {
        public string QuestionnaireId { get; set; }
    }


    [Localizable(false)]
    class Program
    {
        static async Task Main(string[] args)
        {
            var parser = new FluentCommandLineParser<ApplicationArguments>();
            parser.Setup(x => x.ConnectionString)
                .As("cs")
                .Required();

            var parsedArguments = parser.Parse(args);

            var patcher = new DiffMatchPatch.diff_match_patch();

            if (parsedArguments.HasErrors == false)
            {
                using (var connection = new NpgsqlConnection(parser.Object.ConnectionString))
                {
                    await connection.OpenAsync();

                    var questionnairesWithHistoryIds = (await connection.QueryAsync<QuestionnaireIdRecord>(
                        "SELECT DISTINCT questionnaireid FROM plainstore.questionnairechangerecords WHERE resultingquestionnairedocument IS NOT NULL")).ToList();

                    Console.WriteLine($"Found {questionnairesWithHistoryIds.Count} to migrate");

                    foreach (var row in questionnairesWithHistoryIds)
                    {
                        using (var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
                        {
                            try
                            {
                                var existingHistory = await connection.QueryAsync<ChangeRecord>(
                                    @"SELECT id as Id, resultingquestionnairedocument as Questionnaire 
                                  FROM plainstore.questionnairechangerecords 
                                  WHERE questionnaireid = @questionnaireId AND resultingquestionnairedocument IS NOT NULL
                                  ORDER BY ""sequence""", new {row.QuestionnaireId});
                                string reference = existingHistory.First().Questionnaire;

                                foreach (ChangeRecord historyItem in existingHistory.Skip(1))
                                {
                                    var diff = patcher.patch_make(reference, historyItem.Questionnaire ?? string.Empty);
                                    string textPatch = patcher.patch_toText(diff);

                                    var appliedPatch = patcher.patch_apply(diff, reference)[0].ToString();
                                    if (!appliedPatch.Equals(historyItem.Questionnaire))
                                    {
                                        throw new Exception($"Applied patch for questionnaire {row}. Applied patch does not match target questionnaire. Tested change id {historyItem.Id}"); await transaction.RollbackAsync();
                                    }

                                    await connection.ExecuteAsync(@"UPDATE plainstore.questionnairechangerecords 
                                                                    SET resultingquestionnairedocument = NULL, diffwithpreviousversion = @diff 
                                                                    WHERE id = @id",
                                        new
                                        {
                                            diff = textPatch,
                                            id = historyItem.Id
                                        });

                                    reference = historyItem.Questionnaire;
                                }

                                await transaction.CommitAsync();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                await transaction.RollbackAsync();
                            }
                        }
                    }
                }
            }
        }
    }
}

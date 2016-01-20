using CommandLine;
using CommandLine.Text;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;

namespace QToPlainStore
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine("Start.");
                Transfer(options);
                Console.WriteLine("Done.");
            }
        }

        protected class Options
        {
            [Option('r', "readside", Required = true, HelpText = "PostgreSql read side connection string.")]
            public string PGReadSideConnection { get; set; }

            [Option('p', "plainstore", Required = true, HelpText = "PostgreSql plain storage connection string.")]
            public string PGPlainConnection { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
        
        private static void Transfer(Options options)
        {
            var items = GetQuestionnairesFromReadSide(options.PGReadSideConnection);
            Console.WriteLine("Found {0} questionnaires.", items.Count);

            foreach (var item in items)
            {
                Console.WriteLine("Processing {0} questionnaire.", item.Key);
                InsertItemIfNotExists(options.PGPlainConnection, item.Key, item.Value);
            }

        }

        private static Dictionary<string, string> GetQuestionnairesFromReadSide(string PGReadSideConnection)
        {
            var questionnaires = new Dictionary<string,string>();

            using (var connection = new NpgsqlConnection(PGReadSideConnection))
            {
                connection.Open();
                using (connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"SELECT * FROM questionnairedocumentversioneds";
                    using (IDataReader npgsqlDataReader = command.ExecuteReader())
                    {
                        while (npgsqlDataReader.Read())
                        {
                            string value = (string)npgsqlDataReader["value"];
                            string id = (string)npgsqlDataReader["id"];

                            questionnaires.Add(id,value);
                        }
                    }
                }
            }

            return questionnaires;
        }

        private static void InsertItemIfNotExists(string PGPlainConnection, string id, string valueToExtract)
        {
            using (var connection = new NpgsqlConnection(PGPlainConnection))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var checkCommand = connection.CreateCommand();

                    checkCommand.CommandText = $"select exists(select 1 from questionnairedocuments where id=:id)";
                    checkCommand.Parameters.AddWithValue("id", id);

                    var exists = (bool)checkCommand.ExecuteScalar();

                    if (!exists)
                    {
                        var insertCommand = connection.CreateCommand();

                        insertCommand.CommandText = $"INSERT INTO questionnairedocuments VALUES(:id, :value)";

                        //hack do not pull here classes and deserialize full objects
                        var root = JObject.Parse(valueToExtract);
                        var questionnaire = root["Questionnaire"].ToString(Newtonsoft.Json.Formatting.None);

                        if(string.IsNullOrWhiteSpace(questionnaire))
                            throw new Exception("Invalid Questionnaire content.");
                        
                        var valueParameter = new NpgsqlParameter("value", NpgsqlDbType.Json) { Value = questionnaire };
                        var parameter = new NpgsqlParameter("id", NpgsqlDbType.Varchar) { Value = id };
                        insertCommand.Parameters.Add(parameter);
                        insertCommand.Parameters.Add(valueParameter);
                        var queryResult = insertCommand.ExecuteNonQuery();

                        if (queryResult > 1)
                        {
                            throw new Exception(string.Format("Unexpected row count of deleted records. Expected to delete not more than 1 row, but affected {0} number of rows", queryResult));
                        }

                        transaction.Commit();

                        Console.WriteLine("Questionnaire {0} inserted.", id);
                    }
                    else
                    {
                        Console.WriteLine("Questionnaire {0} exists. Ignoring.", id);
                    }
                }
            }
        }
        
    }
}

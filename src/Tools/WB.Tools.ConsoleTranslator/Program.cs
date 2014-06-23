using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

namespace WB.Tools.ConsoleTranslator
{
    class Program
    {
        struct Entry
        {
            public string Variable { get; set; }
            public string Code { get; set; }
            public string InitialLanguage { get; set; }
            public string TargetLanguage { get; set; }
        }

        private static readonly Dictionary<string, Tuple<string, Action<string[]>>> Actions = new Dictionary<string, Tuple<string, Action<string[]>>>
        {
            {
                "help", Tuple.Create<string, Action<string[]>>(
                    string.Empty,
                    args => ShowHelp())
            },
            {
                "get-template", Tuple.Create<string, Action<string[]>>(
                    "questionnaire.json translation-template.csv",
                    args => GetCsvTemplate(args[1], args[2]))
            },
        };

        static void Main(string[] args)
        {
            Bootstrap();

            string inputQuestionnairePath = @"C:\Users\Anatoliy\Desktop\PERCEPTIONS OF INEQUALITY AND ASPIRED ECONOMIC MOBILITY IN NEPAL (SECOND VERSION).tmpl";
            string outputCsvPath = @"C:\Users\Anatoliy\Desktop\PERCEPTIONS OF INEQUALITY.csv";

            if (args.Length == 0)
            {
                args = new [] { "get-template", inputQuestionnairePath, outputCsvPath };
            }

            try
            {
                Action<string[]> action = Actions[args[0]].Item2;
                action.Invoke(args);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Console.WriteLine();
                ShowHelp();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");

            foreach (var action in Actions)
            {
                Console.WriteLine("    {0} {1}", action.Key, action.Value.Item1);
            }

            Console.WriteLine();
        }

        private static void GetCsvTemplate(string inputQuestionnairePath, string outputCsvPath)
        {
            QuestionnaireDocument questionnaire = LoadQuestionnaire(inputQuestionnairePath);

            var entries = ExtractEntriesFromQuestionaire(questionnaire).ToList();

            WriteEntriesToCsv(entries, outputCsvPath);
        }

        private static void Bootstrap()
        {
            var serviceLocator = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object;

            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        private static IEnumerable<Entry> ExtractEntriesFromQuestionaire(QuestionnaireDocument questionnaire)
        {
            foreach (var question in questionnaire.GetAllQuestions<IQuestion>())
            {
                yield return new Entry
                {
                    Variable = question.StataExportCaption,
                    Code = string.Empty,
                    InitialLanguage = question.QuestionText,
                    TargetLanguage = string.Empty,
                };

                foreach (var answerOption in question.Answers)
                {
                    yield return new Entry
                    {
                        Variable = question.StataExportCaption,
                        Code = answerOption.AnswerValue,
                        InitialLanguage = answerOption.AnswerText,
                        TargetLanguage = string.Empty,
                    };
                }
            }
        }

        private static void WriteEntriesToCsv(List<Entry> entries, string path)
        {
            var fileSystemAccessor = new FileSystemIOAccessor();

            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(path, true))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
            using (var writer = new CsvWriter(streamWriter))
            {
                writer.WriteRecord<Entry>(new Entry
                {
                    Variable = "variable",
                    Code = "code",
                    InitialLanguage = "initial_language",
                    TargetLanguage = "target_language",
                });

                foreach (var entry in entries)
                {
                    writer.WriteRecord<Entry>(entry);
                }

                streamWriter.Flush();
            }
        }

        private static QuestionnaireDocument LoadQuestionnaire(string path)
        {
            string fileContent = File.OpenText(path).ReadToEnd();

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var questionaire = JsonConvert.DeserializeObject<QuestionnaireDocument>(fileContent, settings);

            questionaire.PublicKey = Guid.NewGuid();

            return questionaire;
        }
    }
}

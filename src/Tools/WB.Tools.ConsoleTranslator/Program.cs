using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

namespace WB.Tools.ConsoleTranslator
{
    internal class Program
    {
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
            {
                "translate", Tuple.Create<string, Action<string[]>>(
                    "intial-questionnaire.json translation.csv target-questionnaire.json",
                    args => Translate(args[1], args[2], args[3]))
            },
        };

        static void Main(string[] args)
        {
            Bootstrap();

            string inputQuestionnairePath = @"C:\Users\Anatoliy\Desktop\PERCEPTIONS OF INEQUALITY AND ASPIRED ECONOMIC MOBILITY IN NEPAL (SECOND VERSION).tmpl";
            string csvPath = @"C:\Users\Anatoliy\Desktop\PERCEPTIONS OF INEQUALITY.csv";
            string targetQuestionnairePath = @"C:\Users\Anatoliy\Desktop\Translated PERCEPTIONS OF INEQUALITY AND ASPIRED ECONOMIC MOBILITY IN NEPAL (SECOND VERSION).tmpl";

            if (args.Length == 0)
            {
                args = new [] { "translate", inputQuestionnairePath, csvPath, targetQuestionnairePath };
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
            QuestionnaireDocument questionnaire = LoadQuestionnaireFromJsonFile(inputQuestionnairePath);

            List<Entry> entries = ExtractEntriesFromQuestionaire(questionnaire).ToList();

            WriteEntriesToCsv(entries, outputCsvPath);
        }

        private static void Translate(string inputQuestionnairePath, string translationCsvPath, string targetQuestionairePath)
        {
            QuestionnaireDocument questionnaire = LoadQuestionnaireFromJsonFile(inputQuestionnairePath);

            List<Entry> entries = ExtractEntriesFromCsvFile(translationCsvPath).ToList();

            TranslateQuestionnaire(questionnaire, entries);

            WriteQuestionnaireToJsonFile(questionnaire, targetQuestionairePath);
        }

        private static void Bootstrap()
        {
            var serviceLocator = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object;

            ServiceLocator.SetLocatorProvider(() => serviceLocator);
        }

        private static IEnumerable<Entry> ExtractEntriesFromQuestionaire(QuestionnaireDocument questionnaire)
        {
            foreach (var question in questionnaire.GetEntitiesByType<IQuestion>())
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

        private static void TranslateQuestionnaire(QuestionnaireDocument questionnaire, List<Entry> entries)
        {
            int translatedQuestions = 0;
            int skippedQuestions = 0;
            int translatedOptions = 0;
            int skippedOptions = 0;

            foreach (var question in questionnaire.GetEntitiesByType<IQuestion>())
            {
                Entry questionEntry = entries.GetQuestionEntry(question.StataExportCaption);

                if (questionEntry.HasTranslation())
                {
                    question.QuestionText = questionEntry.TargetLanguage;
                    translatedQuestions++;
                }
                else
                {
                    skippedQuestions++;
                }

                foreach (var answerOption in question.Answers)
                {
                    Entry answerOptionEntry = entries.GetAnswerOptionEntry(question.StataExportCaption, answerOption.AnswerValue);

                    if (answerOptionEntry.HasTranslation())
                    {
                        answerOption.AnswerText = answerOptionEntry.TargetLanguage;
                        translatedOptions++;
                    }
                    else
                    {
                        skippedOptions++;
                    }
                }
            }

            Console.WriteLine("Questions translated: {0}", translatedQuestions);
            Console.WriteLine("Questions skipped: {0}", skippedQuestions);
            Console.WriteLine("Options translated: {0}", translatedOptions);
            Console.WriteLine("Options skipped: {0}", skippedOptions);
        }

        private static void WriteEntriesToCsv(List<Entry> entries, string path)
        {
            var fileSystemAccessor = new FileSystemIOAccessor();

            using (var fileStream = fileSystemAccessor.OpenOrCreateFile(path, true))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                csvWriter.WriteHeader<Entry>();

                foreach (var entry in entries)
                {
                    csvWriter.WriteRecord<Entry>(entry);
                }

                streamWriter.Flush();
            }
        }

        private static IEnumerable<Entry> ExtractEntriesFromCsvFile(string path)
        {
            var fileSystemAccessor = new FileSystemIOAccessor();

            using (var fileStream = fileSystemAccessor.ReadFile(path))
            using (var streamReader = new StreamReader(fileStream))
            using (var csvReader = new CsvReader(streamReader))
            {
                while (csvReader.Read())
                {
                    yield return csvReader.GetRecord<Entry>();
                }
            }
        }

        private static QuestionnaireDocument LoadQuestionnaireFromJsonFile(string path)
        {
            string fileContent = File.OpenText(path).ReadToEnd();

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var questionaire = JsonConvert.DeserializeObject<QuestionnaireDocument>(fileContent, settings);

            questionaire.PublicKey = Guid.NewGuid();

            return questionaire;
        }

        private static void WriteQuestionnaireToJsonFile(QuestionnaireDocument questionnaire, string path)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented,
            };

            string fileContent = JsonConvert.SerializeObject(questionnaire, settings);

            File.WriteAllText(path, fileContent, Encoding.UTF8);
        }
    }
}

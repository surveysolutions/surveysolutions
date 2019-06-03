using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace WB.WebInterview.Stress
{
    public class ApiMaster
    {
        private bool isInitiated = false;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public async Task InitAsync(IHubProxy proxy)
        {
            if (isInitiated) return;

            Console.WriteLine("Api Master in lock wait");
            _lock.Wait();
            Console.WriteLine("Api Master in lock wait. Done");

            try
            {
                if (isInitiated) return;

                await InitQuestionaryAsync(proxy);
                isInitiated = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected error during api master init: " + e.Message);
            }
            finally
            {
                _lock.Release();
            }

        }

        static readonly string[] SupportedTypes = { "TextQuestion", "Integer", "Double", "CategoricalSingle" };

        private async Task InitQuestionaryAsync(IHubProxy proxy)
        {
            Console.WriteLine("Collecting questionarie information");

            var sections = await GetSidebarChildSectionsOf(proxy, null, null);
            var innerSection = await GetSidebarChildSectionsOf(proxy, sections.Groups.Select(s => s.Id).ToArray());

            Console.WriteLine($"Collected info on {innerSection.Groups.Count} sections");

            var types = new List<InterviewEntityWithType>();

            foreach (var section in sections.Groups.Union(innerSection.Groups))
            {
                var questions = await GetSectionEntities(proxy, section.Id);
                await GetFullSectionInfo(proxy, section.Id);
                types.AddRange(questions);
            }

            types = types.Where(t => SupportedTypes.Contains(t.EntityType)).ToList();
            var ids = types.Select(q => q.Identity).Distinct().ToArray();
            Console.WriteLine($"Query for details on {ids.Length} questions");

            QuestionTypes = types.Distinct().ToLookup(t => t.Identity, t => t.EntityType);
            Questions = (await GetEntitiesDetails(proxy, ids)).ToList();

            Console.WriteLine($"Query for details on {ids.Length} questions. Done");
        }

        public ILookup<string, string> QuestionTypes { get; set; }

        public List<GenericQuestion> Questions { get; set; } = new List<GenericQuestion>();

        public async Task<Sidebar> GetSidebarChildSectionsOf(IHubProxy proxy, params string[] items)
        {
            return await proxy.Invoke<Sidebar>(nameof(GetSidebarChildSectionsOf), new List<string>(items));
        }

        public async Task<InterviewEntityWithType[]> GetSectionEntities(IHubProxy proxy, string sectionId)
        {
            return await proxy.Invoke<InterviewEntityWithType[]>(nameof(GetSectionEntities), sectionId);
        }

        Dictionary<string, InterviewEntity> QuestionDetails = new Dictionary<string, InterviewEntity>();

        public async Task GetFullSectionInfo(IHubProxy proxy, string sectionId)
        {
            var res = await proxy.Invoke<JObject>(nameof(GetFullSectionInfo), sectionId);

            var detail = res["details"].ToDictionary(d => d["id"].Value<string>());

            foreach (var entity in res["entities"])
            {
                var id = entity["identity"].Value<string>();
                var entityTypeName = entity["entityType"].Value<string>();

                if (questionTypeMap.TryGetValue(entityTypeName, out var entityType))
                {
                    var question = detail[id].ToObject(entityType) as InterviewEntity;
                    QuestionDetails.Add(id, question);
                }
            }
        }

        Dictionary<string, Type> questionTypeMap = new Dictionary<string, Type>
        {
            ["CategoricalSingle"] = typeof(InterviewSingleOptionQuestion)
        };

        public async Task<GenericQuestion[]> GetEntitiesDetails(IHubProxy proxy, string[] ids)
        {
            try
            {
                return await proxy.Invoke<GenericQuestion[]>(nameof(GetEntitiesDetails), new List<string>(ids));
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot get entities details. " + e.Message);
                throw;
            }
        }

        public async Task AnswerTextQuestion(IHubProxy proxy, string id, string text)
        {
            await proxy.Invoke("AnswerTextQuestion", id, text);
        }

        public async Task AnswerDoubleQuestion(IHubProxy proxy, string id, double text)
        {
            await proxy.Invoke("AnswerDoubleQuestion", id, text);
        }

        public async Task AnswerIntegerQuestion(IHubProxy proxy, string id, int text)
        {
            await proxy.Invoke("AnswerIntegerQuestion", id, text);
        }

        public async Task<string> AnswerSingleOptionQuestion(IHubProxy proxy, string id, int answer)
        {
            var details = this.QuestionDetails[id] as InterviewSingleOptionQuestion;
            if (details.Options.Count == 0) return "Skipped";

            var option = details.Options[answer % details.Options.Count];
            await proxy.Invoke("AnswerIntegerQuestion", id, option.Value);
            return $"{option.Value}: {option.Title}";
        }

        public async Task<bool> AnswerRandomQuestionAsync(IHubProxy proxy, Random rnd, ILogger log)
        {
            var question = Questions[rnd.Next(0, Questions.Count - 1)];
            var type = QuestionTypes[question.Id].First();
            var simpleAnswer = rnd.Next(0, 1000000);
            string answer = null;
            try
            {
                switch (type)
                {
                    case "TextQuestion": await AnswerTextQuestion(proxy, question.Id, simpleAnswer.ToString()); break;
                    case "Integer": await AnswerIntegerQuestion(proxy, question.Id, simpleAnswer); break;
                    case "Double": await AnswerDoubleQuestion(proxy, question.Id, simpleAnswer); break;
                    case "CategoricalSingle": answer = await AnswerSingleOptionQuestion(proxy, question.Id, simpleAnswer); break;
                }

                log.Debug("Invoked, {type} ({answer ?? simpleAnswer.ToString()})", type, answer ?? simpleAnswer.ToString());
                return answer == "Skipped" ? false : true;
            }
            catch (Exception e)
            {
                log.Debug("Cannot invoke, {type} ({simpleAnswer}): {e.Message}", type, simpleAnswer, e.Message);
                throw;
            }
        }
    }
}

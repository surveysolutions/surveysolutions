using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace WB.WebInterview.Stress
{
    public class ApiMaster
    {
        private bool isInitiated = false;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public async Task InitAsync(IHubProxy proxy)
        {
            if (isInitiated) return;

            _lock.Wait();

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

        static readonly string[] SupportedTypes = { "TextQuestion", "Integer", "Double" };

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

        public async Task AnswerRandomQuestionAsync(IHubProxy proxy, Random rnd, Action<string> log)
        {
            var question = Questions[rnd.Next(0, Questions.Count - 1)];
            var type = QuestionTypes[question.Id].First();
            var simpleAnswer = rnd.Next(0, 1000000);

            try
            {
                switch (type)
                {
                    case "TextQuestion": await AnswerTextQuestion(proxy, question.Id, simpleAnswer.ToString()); break;
                    case "Integer": await AnswerIntegerQuestion(proxy, question.Id, simpleAnswer); break;
                    case "Double": await AnswerDoubleQuestion(proxy, question.Id, simpleAnswer); break;
                }

                log($"Invoked, {type} ({simpleAnswer})");
            }
            catch (Exception e)
            {
                log($"Cannot invoke, {type} ({simpleAnswer}): {e.Message}");
                throw;
            }
        }
    }
}
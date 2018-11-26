using WB.Core.SharedKernels.DataCollection.Aggregates;
using System.Linq;
using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class StatefullInterviewSearcher : IStatefullInterviewSearcher
    {
        private readonly IInterviewFactory interviewFactory;

        public StatefullInterviewSearcher(IInterviewFactory interviewFactory)
        {
            this.interviewFactory = interviewFactory;

        }

        public SearchResults Search(IStatefulInterview interview, FilterOption[] flags, int skip, int take)
        {
            var stats = new Dictionary<FilterOption, int>();
            var nodes = GetFilteredNodes(flags, interview, stats);

            int taken = 0, skipped = 0, total = 0;
            int searchResultId = 0;

            Identity lastSection = null;

            var results = new SearchResults
            {
                Stats = stats
            };

            SearchResult currentResult = null;

            foreach (var node in nodes)
            {
                IncrementSectionId();

                if (CanTake())
                {
                    if (currentResult == null || currentResult.SectionId != node.Parent.Identity.ToString())
                    {
                        if (currentResult != null)
                        {
                            results.Results.Add(currentResult);
                        }

                        currentResult = NewResult();
                    }
                
                    currentResult.Questions.Add(new Link
                    {
                        Target = node.Identity.ToString(),
                        Title = GetCurrentNodeTitle()
                    });

                    taken++;
                }
                else
                {
                    skipped++;
                }

                total++;

                SearchResult NewResult()
                {
                    return new SearchResult
                    {
                        Id = searchResultId,
                        SectionId = node.Parent.Identity.ToString(),
                        Sections = node.GetBreadcrumbs().ToList()
                    };
                }

                // need to generate consistent search result id, independed on skip/take values
                void IncrementSectionId()
                {
                    if (lastSection == null)
                    {
                        lastSection = node.Parent.Identity;
                    }

                    if (lastSection != node.Parent.Identity)
                    {
                        lastSection = node.Parent.Identity;
                        searchResultId += 1;
                    }
                }

                string GetCurrentNodeTitle()
                {
                    switch (node)
                    {
                        case InterviewTreeQuestion q: return q.Title.ToString();
                        case InterviewTreeStaticText st: return st.Title.ToString();
                    }

                    return null;
                }

                bool CanTake()
                {
                    if (skipped < skip) return false;
                    if (taken < take) return true;

                    return false;
                }
            }

            if (currentResult != null)
            {
                results.Results.Add(currentResult);
            }

            results.TotalCount = total;

            return results;
        }

        public Dictionary<FilterOption, int> GetStatistics(IStatefulInterview interview)
        {
            var stats = new Dictionary<FilterOption, int>();
            this.GetFilteredNodes(Array.Empty<FilterOption>(), interview, stats).ToArray();

            return stats;
        }

        private static readonly Func<InterviewQuestionFilter, bool> FilteringRule = node => 
            node.Is(FilterOption.WithComments)
            && node.Or(FilterOption.Flagged, FilterOption.NotFlagged)
            && node.Or(FilterOption.Valid, FilterOption.Invalid)
            && node.Or(FilterOption.Answered, FilterOption.NotAnswered)
            && node.Or(FilterOption.ForSupervisor, FilterOption.ForInterviewer);

        private static readonly FilterOption[] AllFilterOptions =
            Enum.GetValues(typeof(FilterOption)).Cast<FilterOption>().ToArray();

        private IEnumerable<IInterviewTreeNode> GetFilteredNodes(FilterOption[] flags, IStatefulInterview interview, Dictionary<FilterOption, int> stats)
        {
            var flagged = interviewFactory.GetFlaggedQuestionIds(interview.Id);

            foreach (var option in AllFilterOptions)
            {
                stats.Add(option, 0);
            }
            
            var rule = new InterviewQuestionFilter(
                new HashSet<Identity>(flagged), 
                FilteringRule);

            var nodes = interview.GetAllInterviewNodes()
                .Where(n => n is InterviewTreeQuestion || n is InterviewTreeStaticText);

            var flagsSet = new HashSet<FilterOption>(flags);

            foreach (var node in nodes)
            {
                rule.SetNode(node);
                var found = rule.Evaluate(flagsSet);

                if (found)
                {
                    yield return node;
                    UpdateStats();
                }
            }

            void UpdateStats()
            {
                foreach (var option in AllFilterOptions)
                {
                    var ruleValue = rule.Evaluate(option, false);
                    
                    if (ruleValue) stats[option] += 1;
                }
            }
        }
    }
}

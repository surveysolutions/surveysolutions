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

        public SearchResults Search(IStatefulInterview interview, FilterOption[] flags, long skip, long take)
        {
            var nodes = GetFilteredNodes(flags, interview);

            long taken = 0;
            long skipped = 0;
            long total = 0;

            var results = new SearchResults();
            SearchResult currentResult = null;
            
            foreach (var node in nodes)
            {
                if (currentResult == null)
                {
                    currentResult = NewResult();
                }

                if (CanTake() && currentResult.SectionId != node.Parent.Identity.ToString())
                {
                    results.Results.Add(currentResult);
                    currentResult = NewResult();
                }

                if (CanTake())
                {
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
                        SectionId = node.Parent.Identity.ToString(),
                        Sections = GetBreadcrumbs(node.Parents).ToList()
                    };
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

        private IEnumerable<Link> GetBreadcrumbs(IEnumerable<IInterviewTreeNode> nodeParents)
        {
            foreach (var node in nodeParents)
            {
                var link = new Link() { Target = node.Identity.ToString() };

                if (node is InterviewTreeSection section)
                {
                    link.Title = section.Title.ToString();
                    yield return link;
                    continue;
                }

                if (node is InterviewTreeRoster roster)
                {
                    link.Title = roster.Title.ToString();
                    yield return link;
                    continue;
                }

                if (node is InterviewTreeGroup group)
                {
                    link.Title = @group.Title.ToString();
                    yield return link;
                    continue;
                }
            }
        }

        private static readonly Func<InterviewQuestionFilter, bool> FilteringRule = node => 
            node.Is(FilterOption.WithComments)
            && node.Or(FilterOption.Flagged, FilterOption.NotFlagged)
            && node.Or(FilterOption.Valid, FilterOption.Invalid)
            && node.Or(FilterOption.Answered, FilterOption.NotAnswered)
            && node.Or(FilterOption.ForSupervisor, FilterOption.ForInterviewer);

        private IEnumerable<IInterviewTreeNode> GetFilteredNodes(FilterOption[] flags, IStatefulInterview interview)
        {
            var flagged = interviewFactory.GetFlaggedQuestionIds(interview.Id);

            var rule = new InterviewQuestionFilter(
                new HashSet<Identity>(flagged), 
                new HashSet<FilterOption>(flags), 
                FilteringRule);

            var nodes = interview.GetAllInterviewNodes();

            foreach (var node in nodes)
            {
                if (!(node is InterviewTreeQuestion question)) continue;

                var found = rule.Evaluate(question);

                if (found) yield return node;
            }
        }
    }
}

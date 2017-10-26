using WB.Core.SharedKernels.DataCollection.Aggregates;
using System.Linq;
using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class StatefullInterviewSearcher : IStatefullInterviewSearcher
    {
        public SearchResults Search(IStatefulInterview interview, FilteringFlags[] flags, long skip, long take)
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

        private IEnumerable<IInterviewTreeNode> GetFilteredNodes(FilteringFlags[] flags, IStatefulInterview interview)
        {
            var nodes = interview.GetAllInterviewNodes()
                .Where(n => n is InterviewTreeQuestion
                    || n is InterviewTreeStaticText);

            foreach (var node in nodes)
            {
                if (flags.Any() == false && node is InterviewTreeStaticText)
                {
                    yield return node;
                    continue;
                };

                if (!(node is InterviewTreeQuestion question)) continue;

                foreach (var flag in flags)
                {
                    switch (flag)
                    {
                        case FilteringFlags.Flagged:
                            break;
                        case FilteringFlags.NotFlagged:
                            break;

                        case FilteringFlags.WithComments:
                            if (question.AnswerComments.Any())
                                yield return node;
                            continue;

                        case FilteringFlags.Invalid:
                            if (question.IsValid == false)
                                yield return node;
                            continue;

                        case FilteringFlags.Valid:
                            if (question.IsValid)
                                yield return node;
                            continue;
                        case FilteringFlags.Answered:
                            if (question.IsAnswered())
                                yield return node;
                            continue;
                        case FilteringFlags.Unanswered:
                            if (question.IsAnswered() == false)
                                yield return node;
                            continue;
                        case FilteringFlags.ForSupervisor:
                            if (question.IsSupervisors)
                                yield return node;
                            continue;
                        case FilteringFlags.ForInterviewer:
                            if (question.IsSupervisors == false)
                                yield return node;
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(flags));
                    }
                }
            }
        }
    }
}

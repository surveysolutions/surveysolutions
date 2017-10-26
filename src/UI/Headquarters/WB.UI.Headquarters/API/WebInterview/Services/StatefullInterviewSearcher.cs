using WB.Core.SharedKernels.DataCollection.Aggregates;
using System.Linq;
using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
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

        private IEnumerable<IInterviewTreeNode> GetFilteredNodes(FilteringFlags[] flags, IStatefulInterview interview)
        {
            var flagged = interviewFactory.GetFlaggedQuestionIds(interview.Id).ToLookup(k => k.ToString());

            var nodes = interview.GetAllInterviewNodes();

            foreach (var node in nodes)
            {
                if (!(node is InterviewTreeQuestion question)) continue;

                var found = true;

                foreach (var flag in flags)
                {
                    switch (flag)
                    {
                        case FilteringFlags.Flagged:
                            found &= flagged.Contains(question.Identity.ToString());
                            break;
                        case FilteringFlags.NotFlagged:
                            found &= !flagged.Contains(question.Identity.ToString());
                            break;

                        case FilteringFlags.WithComments:
                            found &= question.AnswerComments.Any();
                            break;

                        case FilteringFlags.Invalid:
                            found &= !question.IsValid;
                            break;

                        case FilteringFlags.Valid:
                            found &= question.IsValid;
                            break;
                        case FilteringFlags.Answered:
                            found &= question.IsAnswered();
                            break;

                        case FilteringFlags.Unanswered:
                            found &= !question.IsAnswered();
                            break;

                        case FilteringFlags.ForSupervisor:
                            found &= question.IsSupervisors;
                            break;

                        case FilteringFlags.ForInterviewer:
                            found &= !question.IsSupervisors;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(flags));
                    }
                }

                if (found) yield return node;
            }
        }
    }
}

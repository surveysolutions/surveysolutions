using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Filtering
{
    public partial class StatefullInterviewSearchTests
    {
        public class FilterTestCase
        {
            public FilterTestCase(params FilterOption[] options)
            {
                Options.AddRange(options);
            }

            public List<FilterOption> Options { get; set; } = new List<FilterOption>();
            public List<Identity> Results { get; set; } = new List<Identity>();
            public int[] StatsCounter { get; set; }
            public long Skip { get; set; }
            public long Take { get; set; }

            public override string ToString()
            {
                return string.Join(", ", Options);
            }

            public FilterTestCase WithOptions(params FilterOption[] options)
            {
                this.Options.AddRange(options);
                return this;
            }

            public FilterTestCase ExpectedQuestions(params Identity[] ids)
            {
                this.Results.AddRange(ids);
                return this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public FilterTestCase ExpectedStats(
                int flagged, int notFlagged, int withComments,
                int invalid, int valid, int answered, int notAnswered,
                int forSupervisor, int forInterviewer, 
                int criticalQuestions, int criticalRules)
            {
                StatsCounter = new[]
                    {flagged, notFlagged, withComments, invalid, valid, answered, notAnswered, forSupervisor, forInterviewer, criticalQuestions, criticalRules};
                return this;
            }
        }
    }
}

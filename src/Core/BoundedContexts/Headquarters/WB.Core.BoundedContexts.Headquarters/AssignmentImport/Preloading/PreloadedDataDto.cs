using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading
{
    public class PreloadedDataDto
    {
        public PreloadedDataDto(PreloadedLevelDto[] data)
        {
            this.Data = data;
        }

        public PreloadedDataDto(List<InterviewAnswer> answers)
        {
            this.Data = answers
                .GroupBy(x => x.Identity.RosterVector)
                .Select(x => new PreloadedLevelDto(x.Key, x.ToDictionary(a => a.Identity.Id, a => a.Answer)))
                .ToArray();
        }

        public PreloadedLevelDto[] Data { get; private set; }

        public List<InterviewAnswer> GetAnswers()
        {
            return Data.SelectMany(x => x.Answers.Select(a => new InterviewAnswer
            {
                Identity = new Identity(a.Key, x.RosterVector),
                Answer = a.Value
            })).ToList();
        }
    }

    public static class InterviewAnswerExtensions
    {
        public static List<InterviewAnswer>[] GroupedByLevels(this IEnumerable<InterviewAnswer> answers) => answers
            .GroupBy(x => x.Identity.RosterVector.Length)
            .Select(x => new { Depth = x.Key, Answers = x.ToList() })
            .OrderBy(x => x.Depth)
            .Select(x => x.Answers)
            .ToArray();
    }
}

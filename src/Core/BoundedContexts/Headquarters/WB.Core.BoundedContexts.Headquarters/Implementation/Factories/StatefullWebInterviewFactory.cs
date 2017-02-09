using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    class StatefullWebInterviewFactory : IStatefullWebInterviewFactory
    {
        private const string humanIdDelimiter = @"-";

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public StatefullWebInterviewFactory(
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryRepository,
            IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.interviewSummaryRepository = interviewSummaryRepository;
            this.statefulInterviewRepository = statefulInterviewRepository;
        }

        public IStatefulInterview Get(string interviewId)
            => this.statefulInterviewRepository.Get(this.GetInterviewIdByHumanId(interviewId) ?? interviewId);

        public string GetInterviewIdByHumanId(string id)
        {
            int humanInterviewId;
            if (!int.TryParse(id.Replace(humanIdDelimiter, ""), out humanInterviewId))
                return id;

            return this.interviewSummaryRepository.Query(
                interviews => interviews.FirstOrDefault(_ => _.HumanId == humanInterviewId))?.SummaryId;
        }

        public string GetHumanInterviewId(string interviewId)
        {
            var humanId = this.interviewSummaryRepository.GetById(interviewId)?.HumanId;
            if (humanId == null) return interviewId;

            var sHumanId = humanId.ToString();
            var humanIdLength = 10;

            if (sHumanId.Length < humanIdLength)
                sHumanId = sHumanId.PadLeft(humanIdLength, '0');

            return $"{sHumanId.Substring(0, 2)}-{sHumanId.Substring(2, 2)}-{sHumanId.Substring(4, 2)}-{sHumanId.Substring(6, 2)}-{sHumanId.Substring(8, 2)}";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal static class InterviewExportedDataDenormalizerShouldExtensions
    {
        public static void ShouldContainInterviewActionLog(this Dictionary<Guid, Tuple<QuestionAnswered, InterviewActionLog>> eventsAndInterviewActionLog, Func<InterviewActionLog, bool> condition)
        {
            foreach (var eventAndInterviewActionLog in eventsAndInterviewActionLog)
            {
                condition(eventAndInterviewActionLog.Value.Item2).ShouldBeTrue();
            }
        }

        public static void ShouldCallStoreForWriter(this Dictionary<Guid, Tuple<QuestionAnswered, InterviewActionLog>> eventsAndInterviewActionLog, Mock<IReadSideRepositoryWriter<InterviewActionLog>> interviewActionLogWriter, Times times)
        {
              foreach (var eventAndInterviewActionLog in eventsAndInterviewActionLog)
            {
                interviewActionLogWriter.Verify(
                    x => x.Store(eventAndInterviewActionLog.Value.Item2, eventAndInterviewActionLog.Key.FormatGuid()), times);
            }
        }
    }
}

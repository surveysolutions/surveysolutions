using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Raven.Client.Embedded;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.Synchronization.Implementation.ReadSide.Indexes;
using WB.Core.Synchronization.SyncStorage;
using WB.Tests.Integration.SurveyManagement.RavenIndexes;

namespace WB.Tests.Integration.Synchronization.RavenIndexes.InterviewSyncPackagesGroupedByRootTests
{
    [Subject(typeof(InterviewSyncPackagesGroupedByRoot))]
    internal class when_applying_index_to_3_misc_InterviewSyncPackageMeta : RavenIndexesTestContext
    {
        Establish context = () =>
        {
            documentStore = CreateDocumentStore(new List<InterviewSyncPackageMeta>()
            {
               CreateInterviewSyncPackageMetaInformation(interviewId1,1,"a",userId),
               CreateInterviewSyncPackageMetaInformation(interviewId1,2,"a",userId),
               CreateInterviewSyncPackageMetaInformation(interviewId2,3,"a",userId)
            }, new InterviewSyncPackagesGroupedByRoot());
        };

        Because of = () =>
            resultItems = QueryUsingIndex<InterviewSyncPackageMeta>(documentStore, typeof(InterviewSyncPackagesGroupedByRoot));

        It should_return_2_lines_of_items = () =>
            resultItems.Length.ShouldEqual(2);

        It should_return_2_diffrent_interview_id = () =>
          resultItems.Select(x => x.InterviewId).ShouldContainOnly(new[] { interviewId1, interviewId2});

        It should_return_top_sort_indexes_for_each_interview_id_ = () =>
          resultItems.Select(x => x.SortIndex).ShouldContainOnly(new long[] { 2, 3 });

        Cleanup stuff = () =>
        {
            documentStore.Dispose();
            documentStore = null;
        };

        private static EmbeddableDocumentStore documentStore;
        private static InterviewSyncPackageMeta[] resultItems;
        private static Guid interviewId1 = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewId2 = Guid.Parse("22222222222222222222222222222222");
        private static Guid userId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

        protected static InterviewSyncPackageMeta CreateInterviewSyncPackageMetaInformation(Guid interviewId, int sortIndex, string itemType, Guid userId)
        {
            return new InterviewSyncPackageMeta(interviewId, Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 1, DateTime.Now, userId, itemType, 10, 5)
            {
                SortIndex = sortIndex,
                PackageId = string.Format("{0}${1}", interviewId.FormatGuid(), sortIndex)
            };
        }
    }
}

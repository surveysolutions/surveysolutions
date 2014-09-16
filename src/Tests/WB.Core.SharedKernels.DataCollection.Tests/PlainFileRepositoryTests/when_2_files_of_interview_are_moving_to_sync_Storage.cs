using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests
{
    internal class when_2_files_of_interview_are_moving_to_sync_storage : PlainFileRepositoryTestContext
    {
        Establish context = () =>
        {
            plainFileRepository = CreatePlainFileRepository();
            plainFileRepository.StoreInterviewBinaryData(interviewId, "file1", new byte[] { 1 });
            plainFileRepository.StoreInterviewBinaryData(interviewId, "file2", new byte[] { 2 });
        };

        Because of = () => plainFileRepository.MoveInterviewsBinaryDataToSyncFolder(interviewId);


        It should_plain_storage_contains_2_files_for_particular_interview = () =>
            plainFileRepository.GetAllIdsOfBinaryDataByInterview(interviewId).Length.ShouldEqual(2);

        It should_sycn_storage_contain_2_files = () =>
            plainFileRepository.GetBinaryFilesFromSyncFolder().Count.ShouldEqual(2);

        It should_sycn_storage_contain_first_file_with_InterviewId_equal_to_interviewId = () =>
           plainFileRepository.GetBinaryFilesFromSyncFolder()[0].InterviewId.ShouldEqual(interviewId);

        It should_sycn_storage_contain_second_file_with_InterviewId_equal_to_interviewId = () =>
          plainFileRepository.GetBinaryFilesFromSyncFolder()[1].InterviewId.ShouldEqual(interviewId);

        private static PlainFileRepository plainFileRepository;
        private static Guid interviewId = Guid.NewGuid();
    }
}

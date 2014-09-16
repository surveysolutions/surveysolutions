using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests
{
    internal class when_2_files_are_stored_for_the_same_interview : PlainFileRepositoryTestContext
    {
        Establish context = () =>
        {
            plainFileRepository = CreatePlainFileRepository();
        };

        Because of = () =>
        {
            plainFileRepository.StoreInterviewBinaryData(interviewId, "file1", new byte[] { 1 });
            plainFileRepository.StoreInterviewBinaryData(interviewId, "file2", new byte[] { 2 });
        };


        It should_plain_storage_contains_2_files_for_particular_interview = () =>
            plainFileRepository.GetAllIdsOfBinaryDataByInterview(interviewId).Length.ShouldEqual(2);

        private static PlainFileRepository plainFileRepository;
        private static Guid interviewId = Guid.NewGuid();
    }
}

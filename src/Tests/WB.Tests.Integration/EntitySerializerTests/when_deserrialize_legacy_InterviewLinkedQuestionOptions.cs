using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Integration.EntitySerializerTests
{
    internal class when_deserrialize_legacy_InterviewLinkedQuestionOptions
    {
        private Establish context = () =>
        {
            options = "{\"LinkedQuestionOptions\":{\"78e523db8474ec8c2c0078f75244c712_0\":[[0.0,1.0],[0.0,2.0]],\"33437136c888ca4b32d29fc7c1d105c5\":[[0.0,1.0],[0.0,2.0]]}}";
            //{"LinkedQuestionOptions":{"78e523db8474ec8c2c0078f75244c712_0":[[0,1],[0,2]],"33437136c888ca4b32d29fc7c1d105c5":[[0,1],[0,2]]}}
            entitySerializer = new  EntitySerializer<InterviewLinkedQuestionOptions>();
        };

        Because of = () 
            => interviewLinkedQuestionOptions = entitySerializer.Deserialize(options);

        It should_return_2_records = () => interviewLinkedQuestionOptions.LinkedQuestionOptions.Count.ShouldEqual(2);

        private static EntitySerializer<InterviewLinkedQuestionOptions> entitySerializer;
        private static string options;
        private static InterviewLinkedQuestionOptions interviewLinkedQuestionOptions;

    }
}


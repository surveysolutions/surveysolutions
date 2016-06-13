using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewDetailsViewFactoryTests
{
    [Subject(typeof(InterviewDetailsViewFactory))]
    internal class InterviewDetailsViewFactoryTestsContext
    {
        protected static InterviewDetailsViewFactory CreateViewFactory(IReadSideKeyValueStorage<InterviewData> interviewStore = null,
            IPlainStorageAccessor<UserDocument> userStore = null,
            IInterviewDataAndQuestionnaireMerger merger = null,
            IChangeStatusFactory changeStatusFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IEventSourcedAggregateRootRepository eventSourcedRepository = null,
            IReadSideKeyValueStorage<InterviewLinkedQuestionOptions> interviewLinkedQuestionOptionsStore = null,
            IAttachmentContentService attachmentContentService = null)
        {
            var users = new Mock<IPlainStorageAccessor<UserDocument>>();
            users.SetReturnsDefault(Create.Entity.UserDocument());

            return new InterviewDetailsViewFactory(interviewStore ?? new TestInMemoryWriter<InterviewData>(),
                userStore ?? users.Object,
                merger ?? Mock.Of<IInterviewDataAndQuestionnaireMerger>(),
                changeStatusFactory ?? Mock.Of<IChangeStatusFactory>(),
                incomingSyncPackagesQueue ?? Mock.Of<IInterviewPackagesService>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                eventSourcedRepository ?? Mock.Of<IEventSourcedAggregateRootRepository>(),
                interviewLinkedQuestionOptionsStore ?? new TestInMemoryWriter<InterviewLinkedQuestionOptions>(),
                attachmentContentService ?? Mock.Of<IAttachmentContentService>());
        }
    }
}
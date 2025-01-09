using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.GeoTracking;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Controllers.Api;
using WB.UI.Headquarters.Models.Api;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentsApiControllerTests
    {
        [Test]
        public void should_ignore_responsible_field_if_authorizedUser_is_interviewer()
        {
            var viewFactory = new Mock<IAssignmentViewFactory>();
            viewFactory.Setup(vf => vf.Load(It.IsAny<AssignmentsInputModel>()))
                .Returns(new AssignmentsWithoutIdentifingData());

            var controller = new AssignmentsApiController(
                viewFactory.Object,
                Mock.Of<IAuthorizedUser>(au => au.IsInterviewer == true && au.Id == Id.gA),
                Mock.Of<IAssignmentsService>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<ISystemLog>(),
                new InMemoryPlainStorageAccessor<QuestionnaireBrowseItem>(),
                Mock.Of<IInvitationService>(),
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IAssignmentPasswordGenerator>(),
                Mock.Of<ICommandService>(),
                Mock.Of<IAssignmentFactory>(),
                Mock.Of<IPlainStorageAccessor<GeoTrackingRecord>>()
            );

            controller.Get(new AssignmentsApiController.AssignmentsDataTableRequest
            {
                Search = new DataTableRequest.SearchInfo(),
                Start = 0,
                Length = 10,
                ResponsibleId = Id.g1
            });

            viewFactory.Verify(vf => vf.Load(It.Is<AssignmentsInputModel>(v => v.ResponsibleId == Id.gA)), Times.Once);
        }
    }
}

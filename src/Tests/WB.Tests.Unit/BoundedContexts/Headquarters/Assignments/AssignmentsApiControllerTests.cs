using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Models.Api;
using WB.UI.Headquarters.Services;

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
                Mock.Of<IPlainStorageAccessor<Assignment>>(),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IInterviewCreatorFromAssignment>(),
                Mock.Of<IAuditLog>(),
                Mock.Of<IPreloadedDataVerifier>(),
                Mock.Of<ICommandTransformator>()
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuantityControllerTests
{
    [Subject(typeof(QuantityController))]
    internal class QuantityControllerTestContext
    {
        protected static QuantityController CreateQuantityController(IGlobalInfoProvider globalInfoProvider=null)
        {
            return new QuantityController(Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(), Mock.Of<ILogger>(),
                Mock.Of<IViewFactory<AllUsersAndQuestionnairesInputModel, AllUsersAndQuestionnairesView>>(
                    _ =>
                        _.Load(Moq.It.IsAny<AllUsersAndQuestionnairesInputModel>()) ==
                        new AllUsersAndQuestionnairesView() {Questionnaires = new TemplateViewItem[0]}));
        }
    }
}

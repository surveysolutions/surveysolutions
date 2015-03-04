using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.TeamUsersAndQuestionnairesFactoryTests
{
    [Subject(typeof(TeamUsersAndQuestionnairesFactory))]
    internal class when_unlocked_interviewers_and_active_templates_should_be_returned
    {
        Establish context = () =>
        {
            var indexAccessorMock = new Mock<IReadSideRepositoryIndexAccessor>();

            indexAccessorMock.Setup(x => x.Query<QuestionnaireAndVersionsItem>(questionnaireIndexName))
                .Returns(
                    new[]
                    {
                        new QuestionnaireAndVersionsItem()
                        {
                            Title = "q1",
                            QuestionnaireId = questionnaireId1,
                            Versions = new long[] {2, 3}
                        },
                          new QuestionnaireAndVersionsItem()
                        {
                            Title = "q2",
                            QuestionnaireId = questionnaireId2,
                            Versions = new long[] {1}
                        }
                    }.AsQueryable());

            indexAccessorMock.Setup(x => x.Query<UserDocument>(userIndexName))
                .Returns(
                    new[]
                    {
                        new UserDocument()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsLockedByHQ = false,
                            IsLockedBySupervisor = false,
                            IsDeleted = false,
                            Roles = new List<UserRoles> {UserRoles.Operator},
                            Supervisor = new UserLight(Guid.NewGuid(),"other")
                        },
                         new UserDocument()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsLockedByHQ = true,
                            IsLockedBySupervisor = false,
                            IsDeleted = false,
                            Roles = new List<UserRoles> {UserRoles.Operator},
                            Supervisor = new UserLight(vieweverId,"correct")
                        },
                         new UserDocument()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsLockedByHQ = false,
                            IsLockedBySupervisor = true,
                            IsDeleted = false,
                            Roles = new List<UserRoles> {UserRoles.Operator},
                            Supervisor = new UserLight(vieweverId,"correct")
                        },
                         new UserDocument()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsLockedByHQ = false,
                            IsLockedBySupervisor = false,
                            IsDeleted = true,
                            Roles = new List<UserRoles> {UserRoles.Operator},
                            Supervisor = new UserLight(vieweverId,"correct")
                        },
                         new UserDocument()
                        {
                            PublicKey = Guid.NewGuid(),
                            IsLockedByHQ = false,
                            IsLockedBySupervisor = false,
                            IsDeleted = false,
                            Roles = new List<UserRoles> {UserRoles.Undefined},
                            Supervisor = new UserLight(vieweverId,"correct")
                        },
                         new UserDocument()
                        {
                            PublicKey = interviewerId,
                            IsLockedByHQ = false,
                            IsLockedBySupervisor = false,
                            IsDeleted = false,
                            Roles = new List<UserRoles> {UserRoles.Operator},
                            Supervisor = new UserLight(vieweverId,"correct")
                        }
                    }.AsQueryable());

            teamUsersAndQuestionnairesFactory = new TeamUsersAndQuestionnairesFactory(indexAccessorMock.Object);
        };

        Because of = () =>
           result = teamUsersAndQuestionnairesFactory.Load(new TeamUsersAndQuestionnairesInputModel(vieweverId));

        It should_return_result_with_3_questionnaire_templates = () =>
           result.Questionnaires.Count().ShouldEqual(3);

        It should_return_result_with_2_different_questionnaire_ids = () =>
           result.Questionnaires.Select(x => x.TemplateId).ShouldEqual(new[] { questionnaireId1,questionnaireId1, questionnaireId2});

        It should_return_result_with_3_different_questionnaire_ids = () =>
         result.Questionnaires.Select(x => x.TemplateVersion).ShouldEqual(new long[] { 2, 3, 1 });

        It should_return_result_with_1_interviewer = () =>
           result.Users.Count().ShouldEqual(1);

        It should_return_interviewer_which_is_unlocked_undeleted_in_operator_role_and_has_supervisor_equal_to_viewerId = () =>
            result.Users.First().UserId.ShouldEqual(interviewerId);

        private static TeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory;
        private static TeamUsersAndQuestionnairesView result;
        private static Guid vieweverId = Guid.Parse("11111111111111111111111111111111");
        private static Guid interviewerId = Guid.Parse("44444444444444444444444444444444");
        private static Guid questionnaireId1 = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaireId2 = Guid.Parse("33333333333333333333333333333333");

        private static string questionnaireIndexName = typeof(QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex).Name;
        private static string userIndexName = typeof(UserDocumentsByBriefFields).Name;
    }
}

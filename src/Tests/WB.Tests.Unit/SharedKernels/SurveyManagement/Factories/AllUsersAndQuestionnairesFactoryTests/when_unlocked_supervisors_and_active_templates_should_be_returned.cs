using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using NSubstitute;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.AllUsersAndQuestionnairesFactoryTests
{
    [Subject(typeof(AllUsersAndQuestionnairesFactory))]
    [Ignore("Postgre")]
    class when_unlocked_supervisors_and_active_templates_should_be_returned
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

            //indexAccessorMock.Setup(x => x.Query<UserDocument>(userIndexName))
            //    .Returns(
            //        new[]
            //        {
            //             new UserDocument()
            //            {
            //                PublicKey = Guid.NewGuid(),
            //                IsLockedByHQ = true,
            //                IsDeleted = false,
            //                Roles = new HashSet<UserRoles> {UserRoles.Supervisor}
            //            },
            //             new UserDocument()
            //            {
            //                PublicKey = Guid.NewGuid(),
            //                IsLockedByHQ = false,
            //                IsDeleted = true,
            //                Roles = new HashSet<UserRoles> {UserRoles.Supervisor}
            //            },
            //             new UserDocument()
            //            {
            //                PublicKey = Guid.NewGuid(),
            //                IsLockedByHQ = false,
            //                IsDeleted = false,
            //                Roles = new HashSet<UserRoles> {UserRoles.Undefined}
            //            },
            //             new UserDocument()
            //            {
            //                PublicKey = supervisorId,
            //                IsLockedByHQ = false,
            //                IsDeleted = false,
            //                Roles = new HashSet<UserRoles> {UserRoles.Supervisor}
            //            }
            //        }.AsQueryable());

            //allUsersAndQuestionnairesFactory = new AllUsersAndQuestionnairesFactory(indexAccessorMock.Object);
        };

        Because of = () =>
           result = allUsersAndQuestionnairesFactory.Load(new AllUsersAndQuestionnairesInputModel());

        It should_return_result_with_3_questionnaire_templates = () =>
           result.Questionnaires.Count().ShouldEqual(3);

        It should_return_result_with_2_different_questionnaire_ids = () =>
           result.Questionnaires.Select(x => x.TemplateId).ShouldEqual(new[] { questionnaireId1, questionnaireId1, questionnaireId2 });

        It should_return_result_with_3_different_questionnaire_ids = () =>
         result.Questionnaires.Select(x => x.TemplateVersion).ShouldEqual(new long[] { 2, 3, 1 });

        It should_return_result_with_1_supervisor = () =>
           result.Users.Count().ShouldEqual(1);

        It should_return_supervisor_which_is_unlocked_undeleted_in_supervisor_role = () =>
            result.Users.First().UserId.ShouldEqual(supervisorId);

        private static AllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory;
        private static AllUsersAndQuestionnairesView result;
        private static Guid supervisorId = Guid.Parse("44444444444444444444444444444444");
        private static Guid questionnaireId1 = Guid.Parse("22222222222222222222222222222222");
        private static Guid questionnaireId2 = Guid.Parse("33333333333333333333333333333333");

        private static string questionnaireIndexName = typeof(QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex).Name;
    }
}

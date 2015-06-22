using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    public class when_selecting_object_in_cascading_view_model_set_in_null : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            var childAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 3);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            var userIdentity = Mock.Of<IUserIdentity>(_ => _.UserId == userId);
            var principal = Mock.Of<IPrincipal>(_ => _.CurrentUserIdentity == userIdentity);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetSingleOptionAnswer(questionIdentity) == childAnswer
                   && _.GetSingleOptionAnswer(parentIdentity) == parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var cascadingQuestionModel = Mock.Of<CascadingSingleOptionQuestionModel>(_
                => _.Id == questionIdentity.Id
                   && _.Options == Options
                   && _.CascadeFromQuestionId == parentIdentity.Id
                   && _.RosterLevelDepthOfParentQuestion == 1);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ => _.Questions == new Dictionary<Guid, BaseQuestionModel> { { questionIdentity.Id, cascadingQuestionModel } });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.Init(interviewId, questionIdentity, navigationState);
        };

        Because of = () =>
            cascadingModel.SelectedObject = null;

        It should_send_answer_command = () =>
            AnsweringViewModelMock.Verify(x => x.SendAnswerQuestionCommand(Moq.It.IsAny<AnswerSingleOptionQuestionCommand>()), Times.Never);

        It should_set_not_null_SelectedObject = () =>
            cascadingModel.SelectedObject.ShouldBeNull();
        
        private static CascadingSingleOptionQuestionViewModel cascadingModel;
    }
}
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.QuestionHeaderViewModelTests
{
    [Subject(typeof(QuestionHeaderViewModel))]
    public class QuestionHeaderViewModelTestsContext
    {
        public static QuestionHeaderViewModel CreateViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            ILiteEventRegistry registry = null,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService = null)
        {
            if (rosterTitleSubstitutionService == null)
            {
                var substStub = new Mock<IRosterTitleSubstitutionService>();
                substStub.Setup(x => x.Substitute(Moq.It.IsAny<string>(), Moq.It.IsAny<Identity>(), Moq.It.IsAny<string>()))
                    .Returns<string, Identity, string>((title, id, interviewId) => title);
                rosterTitleSubstitutionService = substStub.Object;
            }

            return new QuestionHeaderViewModel(questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                registry ?? Create.LiteEventRegistry(),
                new SubstitutionService(),
                new AnswerToStringService(),
                rosterTitleSubstitutionService);
        }
    }
}
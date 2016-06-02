﻿using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    [Subject(typeof(QuestionHeaderViewModel))]
    internal class QuestionHeaderViewModelTestsContext
    {
        public static QuestionHeaderViewModel CreateViewModel(IPlainQuestionnaireRepository questionnaireRepository = null, 
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

            return new QuestionHeaderViewModel(questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                registry ?? Create.Service.LiteEventRegistry(),
                new SubstitutionViewModel(
                    interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                    questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                    new SubstitutionService(),
                    new AnswerToStringService(),
                    new VariableToUIStringService(),
                    rosterTitleSubstitutionService));
        }
    }
}
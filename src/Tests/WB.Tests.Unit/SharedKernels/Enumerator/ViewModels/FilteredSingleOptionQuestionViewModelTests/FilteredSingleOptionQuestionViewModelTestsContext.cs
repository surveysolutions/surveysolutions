using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using MvvmCross.Base;
using MvvmCross.Tests;
using MvvmCross.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredSingleOptionQuestionViewModelTests
{
    internal class FilteredSingleOptionQuestionViewModelTestsContext : MvxIoCSupportingTest
    {
        public FilteredSingleOptionQuestionViewModelTestsContext()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
        }

        protected static FilteredSingleOptionQuestionViewModel CreateFilteredSingleOptionQuestionViewModel(
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            IPrincipal principal = null,
            IStatefulInterviewRepository interviewRepository = null,
            FilteredOptionsViewModel filteredOptionsViewModel = null,
            IQuestionnaireStorage questionnaireStorage = null)
        {
            return new FilteredSingleOptionQuestionViewModel(
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<ILiteEventRegistry>(),
                filteredOptionsViewModel ?? Mock.Of<FilteredOptionsViewModel>(),
                principal ?? Mock.Of<IPrincipal>(),
                questionStateViewModel ?? Mock.Of<QuestionStateViewModel<SingleOptionQuestionAnswered>>(),
                answering ?? Mock.Of<AnsweringViewModel>(),
                Mock.Of<QuestionInstructionViewModel>());
        }
        
        protected static ReadOnlyCollection<CategoricalOption> Options = new List<CategoricalOption>
        {
            Create.Entity.CategoricalQuestionOption(1, "title abc 1"),
            Create.Entity.CategoricalQuestionOption(2, "title def 2"),
            Create.Entity.CategoricalQuestionOption(3, "title klo 3"),
            Create.Entity.CategoricalQuestionOption(4, "title gha 4"),
            Create.Entity.CategoricalQuestionOption(5, "title ccc 5"),
            Create.Entity.CategoricalQuestionOption(6, "title bcw 6")
        }.ToReadOnlyCollection();

        protected static IOptionsRepository SetupOptionsRepositoryForQuestionnaire(Guid questionId, List<CategoricalOption> optionList = null)
        {
            var options = optionList ?? new List<CategoricalOption>
            {
                Create.Entity.CategoricalQuestionOption(1, "abc"),
                Create.Entity.CategoricalQuestionOption(2, "bbc"),
                Create.Entity.CategoricalQuestionOption(3, "bbc"),
                Create.Entity.CategoricalQuestionOption(4, "bbaé"),
                Create.Entity.CategoricalQuestionOption(5, "cccé"),
            };

            Mock<IOptionsRepository> optionsRepository = new Mock<IOptionsRepository>();

            optionsRepository
                .Setup(x => x.GetQuestionOption(questionnaireId, questionId, It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<Guid>()))
                .Returns((QuestionnaireIdentity a, Guid b, int c) => options.First(x => x.Value == c));

            optionsRepository
                .Setup(x => x.GetFilteredQuestionOptions(questionnaireId, questionId, It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .Returns((QuestionnaireIdentity a, Guid b, int? c, string d) => 
                        options.Where(x => x.Title.IndexOf(d ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0).Select(x => x));
            
            return optionsRepository.Object;
        }

        protected static readonly Identity questionIdentity = Create.Entity.Identity(Id.g6, Create.Entity.RosterVector());

        protected static readonly QuestionnaireIdentity questionnaireId = Create.Entity.QuestionnaireIdentity(Id.g9, 1);
    }
}

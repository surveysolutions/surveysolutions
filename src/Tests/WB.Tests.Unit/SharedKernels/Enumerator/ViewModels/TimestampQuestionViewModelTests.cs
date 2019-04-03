﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(TimestampQuestionViewModel))]
    public class TimestampQuestionViewModelTests
    {
        [Test]
        public async Task should_specify_unspecified_timezone_in_answer()
        {
            var commandService = new Mock<ICommandService>();
            var answeringViewModel = Create.ViewModel.AnsweringViewModel(commandService.Object);

            var interview = Mock.Of<IStatefulInterview>(x => x.GetDateTimeQuestion(It.IsAny<Identity>()) ==
                                                             new InterviewTreeDateTimeQuestion());


            var viewModel = Create.ViewModel.TimestampQuestionViewModel(interview, answeringViewModel);
            viewModel.Init(Guid.NewGuid().FormatGuid(), Create.Identity(), Create.Other.NavigationState());

            // Act
            await viewModel.SaveAnswerCommand.ExecuteAsync();

            // Assert
            commandService.Verify(x => x.ExecuteAsync(It.Is<ICommand>(c => ((AnswerDateTimeQuestionCommand)c).Answer.Kind == DateTimeKind.Unspecified), null, It.IsAny<CancellationToken>()), Times.Once,
              "Command should contain unspecified date kind so that timezone is not serialized in json");
        }
    }
}

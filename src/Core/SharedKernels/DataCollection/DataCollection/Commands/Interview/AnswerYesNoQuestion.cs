using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerYesNoQuestion : AnswerQuestionCommand
    {
        public ReadOnlyCollection<YesNoAnswer> AnsweredOptions { get; private set; }

        public AnswerYesNoQuestion(Guid interviewId, Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, IEnumerable<YesNoAnswer> answeredOptions)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.AnsweredOptions = answeredOptions.ToReadOnlyCollection();
        }
    }
}
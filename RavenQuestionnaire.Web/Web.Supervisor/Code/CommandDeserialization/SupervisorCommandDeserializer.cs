using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Shared.Web.CommandDeserialization;
using Web.Supervisor.Code.CommandTransformation;

namespace Web.Supervisor.Code.CommandDeserialization
{
    internal class SupervisorCommandDeserializer : CommandDeserializer
    {
        protected override Dictionary<string, Type> KnownCommandTypes
        {
            get
            {
                return new Dictionary<string, Type>
                    {
                        { "CreateInterviewCommand", typeof (CreateInterviewControllerCommand) },
                        { "DeleteInterviewCommand", typeof (DeleteInterviewCommand) },
                        //assign
                        { "AssignInterviewerCommand", typeof (AssignInterviewerCommand) },
                        { "AssignSupervisorCommand", typeof (AssignSupervisorCommand) },
                        // flags and comments
                        { "SetFlagToAnswer", typeof (SetFlagToAnswerCommand) },
                        { "RemoveFlagFromAnswer ", typeof (RemoveFlagFromAnswerCommand ) },
                        { "CommentAnswerCommand", typeof (CommentAnswerCommand ) },
                        // answer question
                        { "AnswerDateTimeQuestionCommand", typeof (AnswerDateTimeQuestionCommand ) },
                        { "AnswerMultipleOptionsQuestionCommand", typeof (AnswerMultipleOptionsQuestionCommand ) },
                        { "AnswerNumericQuestionCommand", typeof (AnswerNumericQuestionCommand ) },
                        { "AnswerSingleOptionQuestionCommand", typeof (AnswerSingleOptionQuestionCommand ) },
                        { "AnswerTextQuestionCommand", typeof (AnswerTextQuestionCommand ) },
                        // statuses
                        { "ApproveInterviewCommand", typeof (ApproveInterviewCommand ) },
                        { "RejectInterviewCommand", typeof ( RejectInterviewCommand) },
                    };
            }
        }
    }
}

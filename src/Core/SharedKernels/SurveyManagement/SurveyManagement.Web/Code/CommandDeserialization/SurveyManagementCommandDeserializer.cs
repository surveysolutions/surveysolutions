using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization
{
    internal class SurveyManagementCommandDeserializer : CommandDeserializer
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
                        { "SetFlagToAnswerCommand", typeof (SetFlagToAnswerCommand) },
                        { "RemoveFlagFromAnswerCommand", typeof (RemoveFlagFromAnswerCommand ) },
                        { "CommentAnswerCommand", typeof (CommentAnswerCommand ) },
                        // answer question
                        { "AnswerDateTimeQuestionCommand", typeof (AnswerDateTimeQuestionCommand ) },
                        { "AnswerMultipleOptionsQuestionCommand", typeof (AnswerMultipleOptionsQuestionCommand ) },
                        { "AnswerNumericRealQuestionCommand", typeof (AnswerNumericRealQuestionCommand ) },
                        { "AnswerNumericIntegerQuestionCommand", typeof (AnswerNumericIntegerQuestionCommand ) },
                        { "AnswerSingleOptionQuestionCommand", typeof (AnswerSingleOptionQuestionCommand ) },
                        { "AnswerTextQuestionCommand", typeof (AnswerTextQuestionCommand ) },
                        { "AnswerGeoLocationQuestionCommand", typeof(AnswerGeoLocationQuestionCommand)},
                        // statuses
                        { "ApproveInterviewCommand", typeof (ApproveInterviewCommand ) },
                        { "RejectInterviewCommand", typeof ( RejectInterviewCommand) },

                        { "HqApproveInterviewCommand", typeof (HqApproveInterviewCommand ) },
                        { "HqRejectInterviewCommand", typeof ( HqRejectInterviewCommand) },
                        { "ArchiveUserCommad", typeof ( ArchiveUserCommad) },
                        { "UnarchiveUserCommand", typeof ( UnarchiveUserCommand) }
                    };
            }
        }
    }
}
